using OpenBullet.Pages.StackerBlocks;
using OpenBullet.ViewModels;
using RuriLib;
using RuriLib.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace OpenBullet
{
    /// <summary>
    /// Logica di interazione per ConfigManager.xaml
    /// </summary>

    public partial class ConfigManager : Page
    {
        public ConfigManagerViewModel vm = new ConfigManagerViewModel();
        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;
        public ConfigViewModel Current { get { return Globals.mainWindow.ConfigsPage.CurrentConfig; } set { Globals.mainWindow.ConfigsPage.CurrentConfig = value; vm.RefreshCurrent(); } }

        public void OnSaveConfig(object sender, EventArgs e)
        {
            saveConfigButton_Click(this, new RoutedEventArgs());
        }

        public ConfigManager()
        {
            InitializeComponent();
            DataContext = vm;

            vm.RefreshList();
        }

        public bool CheckSaved()
        {
            var stacker = Globals.mainWindow.ConfigsPage.StackerPage;
            if (Current == null || Globals.obSettings.General.DisableNotSavedWarning || stacker == null) return true;
            stacker.SetScript();
            var cvm = stacker.vm.Config;
            if (string.IsNullOrEmpty(vm.SavedConfig) || cvm == null) return true;
            return vm.SavedConfig == IOManager.SerializeConfig(cvm.Config);
        }

        public void SaveState()
        {
            var cvm = Globals.mainWindow.ConfigsPage.StackerPage.vm.Config;
            if (cvm != null) vm.SavedConfig = IOManager.SerializeConfig(cvm.Config);
        }
        
        private void loadConfigButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckSaved())
            {
                Globals.LogWarning(Components.Stacker, "Config not saved, prompting quit confirmation");
                if (MessageBox.Show("The Config in Stacker wasn't saved.\nAre you sure you want to load another config?",
                    "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                    return;
            }

            // Create new instance of stacker
            Current = (ConfigViewModel)configsListView.SelectedItem; // Set Current Config

            if (Current.Remote)
            {
                Globals.LogError(Components.ConfigManager, "The config was pulled from a remote source and cannot be edited!", true);
                return;
            }

            if (Current != null)
            {
                Globals.LogInfo(Components.ConfigManager, "Loading config: " + Current.Name);

                Globals.mainWindow.ConfigsPage.menuOptionStacker.IsEnabled = true;
                Globals.mainWindow.ConfigsPage.menuOptionOtherOptions.IsEnabled = true;
                var newStacker = new Stacker(Current);
                if (Globals.mainWindow.ConfigsPage.StackerPage != null)
                {
                    newStacker.vm.TestData = Globals.mainWindow.ConfigsPage.StackerPage.vm.TestData;
                    newStacker.vm.TestProxy = Globals.mainWindow.ConfigsPage.StackerPage.vm.TestProxy;
                    newStacker.vm.ProxyType = Globals.mainWindow.ConfigsPage.StackerPage.vm.ProxyType;
                }
                Globals.mainWindow.ConfigsPage.StackerPage = newStacker; // Create a Stacker instance
                Globals.LogInfo(Components.ConfigManager, "Created and assigned a new Stacker instance");
                Globals.mainWindow.ConfigsPage.OtherOptionsPage = new ConfigOtherOptions(); // Create an Other Options instance
                Globals.LogInfo(Components.ConfigManager, "Created and assigned a new Other Options instance");
                Globals.mainWindow.ConfigsPage.menuOptionStacker_MouseDown(this, null); // Switch to Stacker

                // Save the last state of the config
                Globals.mainWindow.ConfigsPage.StackerPage.SetScript();
                SaveState();
            }
            else
            {
                Globals.LogError(Components.ConfigManager, "No config selected for loading", true);
            }
        }

        
        private void saveConfigButton_Click(object sender, RoutedEventArgs e)
        {
            if (Current == null)
            {
                Globals.LogError(Components.ConfigManager, "No config selected for saving!");
                return;
            }

            SaveConfig();
        }

        
        public void SaveConfig()
        {
            if (Globals.mainWindow.ConfigsPage.CurrentConfig == null ||
                Globals.mainWindow.ConfigsPage.StackerPage == null ||
                Globals.mainWindow.ConfigsPage.OtherOptionsPage == null)
            {
                Globals.LogError(Components.ConfigManager, "No config eligible for saving!", true);
                return;
            }

            if (Current.Remote)
            {
                Globals.LogError(Components.ConfigManager, "The config was pulled from a remote source and cannot be saved!", true);
                return;
            }

            if (vm.CurrentConfigName == "") {
                Globals.LogError(Components.ConfigManager, "Empty config name, cannot save", true);
                return;
            }

            var stacker = Globals.mainWindow.ConfigsPage.StackerPage.vm;
            stacker.ConvertKeychains();

            if (stacker.View == StackerView.Blocks)
                stacker.LS.FromBlocks(stacker.GetList());

            Current.Config.Script = stacker.LS.Script;

            Globals.LogInfo(Components.ConfigManager, $"Saving config {vm.CurrentConfigName}");

            Current.Config.Settings.LastModified = DateTime.Now;
            Current.Config.Settings.Version = Globals.obVersion;
            Globals.LogInfo(Components.ConfigManager, "Converted the unbinded observables and set the Last Modified date");
            
            // Save to file            
            if (!IOManager.SaveConfig(Current.Config, Current.Path)) {
                Globals.LogError(Components.ConfigManager, "Failed to save the config to file.", true);
                return;
            };

            // Save the last state of the config
            SaveState();

            Globals.LogInfo(Components.ConfigManager, "Refreshing the list");
            vm.RefreshList();
        }
        
        private void deleteConfigsButton_Click(object sender, RoutedEventArgs e)
        {
            Globals.LogWarning(Components.ConfigManager, "Deletion initiated, prompting warning");
            if (MessageBox.Show("This will delete the physical files from your disk! Are you sure you want to continue?", "WARNING", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                foreach(ConfigViewModel config in configsListView.SelectedItems)
                {
                    try
                    {
                        File.Delete(config.Path);
                    }
                    catch { Globals.LogError(Components.ConfigManager, "Could not delete file: " + config.Path); }
                }

                Globals.LogInfo(Components.ConfigManager, $"Deleted {configsListView.SelectedItems.Count} configs");
                vm.RefreshList();
            }
            else
            {
                Globals.LogInfo(Components.ConfigManager, "Deletion cancelled");
            }
        }

        private void rescanConfigsButton_Click(object sender, RoutedEventArgs e)
        {
            vm.RefreshList();
        }

        private void newConfigButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckSaved())
            {
                Globals.LogWarning(Components.Stacker, "Config not saved, prompting quit confirmation");
                if (MessageBox.Show("The Config in Stacker wasn't saved.\nAre you sure you want to create a new config?",
                    "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                    return;
            }

            (new MainDialog(new DialogNewConfig(this), "New Config")).ShowDialog();
        }
        
        public void CreateConfig(string name, string category, string author)
        {
            // Build the filename
            var path = Globals.configFolder + "\\" + (category == "Default" ? "" : (category + "\\")) + name + ".loli";

            // Create the Category folder if it doesn't exist
            if(category != "Default")
            {
                var categoryFolder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), Globals.configFolder, category);                
                if (!Directory.Exists(categoryFolder))
                    Directory.CreateDirectory(categoryFolder);
            }

            // Build the base config structure
            var settings = new ConfigSettings();
            settings.Name = name;
            settings.Author = author;
            Globals.LogInfo(Components.ConfigManager, "Setting up the new Config object, with path '" + path + "'");
            Current = new ConfigViewModel(path, category, new Config(settings, ""));

            var newStacker = new Stacker(Current);
            if (Globals.mainWindow.ConfigsPage.StackerPage != null) // Maintain the previous stacker settings
            {
                newStacker.vm.TestData = Globals.mainWindow.ConfigsPage.StackerPage.vm.TestData;
                newStacker.vm.TestProxy = Globals.mainWindow.ConfigsPage.StackerPage.vm.TestProxy;
                newStacker.vm.ProxyType = Globals.mainWindow.ConfigsPage.StackerPage.vm.ProxyType;
            }
            Globals.mainWindow.ConfigsPage.StackerPage = newStacker; // Create a Stacker instance
            Globals.LogInfo(Components.ConfigManager, "Created and assigned a new Stacker instance");
            Globals.mainWindow.ConfigsPage.OtherOptionsPage = new ConfigOtherOptions(); // Create an Other Options instance
            Globals.LogInfo(Components.ConfigManager, "Created and assigned a new Other Options instance");
            Globals.mainWindow.ConfigsPage.menuOptionStacker_MouseDown(this, null); // Switch to Stacker

            // Save to disk
            saveConfigButton_Click(this, null);

            vm.RefreshList();

            // Create new instance of stacker
            Globals.mainWindow.ConfigsPage.StackerPage = new Stacker(Current);
            Globals.mainWindow.ConfigsPage.menuOptionStacker_MouseDown(this, null); // Switch to Stacker
        }

        private void configsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try { vm.MoreInfoConfig = ((ConfigViewModel)configsListView.SelectedItem).Config; } catch { }
        }

        private void listViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            string sortBy = column.Tag.ToString();
            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                configsListView.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            configsListView.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            vm.SearchString = filterTextbox.Text;
            if(vm.SearchString == "")
                vm.RefreshList();
        }

        private void filterTextbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                searchButton_Click(this, null);
        }

        private void openConfigFolderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(System.IO.Path.Combine(Directory.GetCurrentDirectory(), Globals.configFolder));
            }
            catch { Globals.LogError(Components.ConfigManager, "No config folder found!", true); }
        }

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            loadConfigButton_Click(this, new RoutedEventArgs());
        }
    }
}
