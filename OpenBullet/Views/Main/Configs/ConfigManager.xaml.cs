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

namespace OpenBullet.Views.Main.Configs
{
    /// <summary>
    /// Logica di interazione per ConfigManager.xaml
    /// </summary>

    public partial class ConfigManager : Page
    {
        public ConfigManagerViewModel vm = new ConfigManagerViewModel();
        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;

        private IEnumerable<ConfigViewModel> Selected => configsListView.SelectedItems.Cast<ConfigViewModel>();

        public void OnSaveConfig(object sender, EventArgs e)
        {
            saveConfigButton_Click(this, new RoutedEventArgs());
        }

        public ConfigManager()
        {
            InitializeComponent();
            DataContext = vm;
        }

        #region State
        // Checks if the config's hash is the same as the saved one
        public bool CheckSaved()
        {
            var stacker = Globals.mainWindow.ConfigsPage.StackerPage;
            
            // If we don't have a config selected or we suppressed the warning or we don't have a Stacker open
            if (vm.CurrentConfig == null || Globals.obSettings.General.DisableNotSavedWarning || stacker == null)
            {
                return true;
            }

            // Blocks to LS conversion because we are going to hash the LS
            stacker.SetScript();
            var cvm = stacker.vm.Config;

            // If we don't have a config loaded in Stacker
            if (cvm == null)
            {
                return true;
            }

            return vm.SavedHash == cvm.Config.Script.GetHashCode();
        }

        // Saves a hash of the LS of the config in Stacker
        public void SaveState()
        {
            var cvm = Globals.mainWindow.ConfigsPage.StackerPage.vm.Config;

            if (cvm != null)
            {
                vm.SavedHash = cvm.Config.Script.GetHashCode();
            }
        }
        #endregion

        #region Buttons
        private void loadConfigButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckSaved())
            {
                Globals.logger.LogWarning(Components.Stacker, "Config not saved, prompting quit confirmation");
                if (MessageBox.Show("The Config in Stacker wasn't saved.\nAre you sure you want to load another config?",
                    "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                    return;
            }

            // Load the config
            LoadConfig(configsListView.SelectedItem as ConfigViewModel);
        }

        private void saveConfigButton_Click(object sender, RoutedEventArgs e)
        {
            SaveConfig();
        }

        private void deleteConfigsButton_Click(object sender, RoutedEventArgs e)
        {
            Globals.logger.LogWarning(Components.ConfigManager, "Deletion initiated, prompting warning");
            if (MessageBox.Show("This will delete the physical files from your disk! Are you sure you want to continue?", "WARNING", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                vm.Remove(Selected);

                Globals.logger.LogInfo(Components.ConfigManager, "Deletion completed");
            }
            else
            {
                Globals.logger.LogInfo(Components.ConfigManager, "Deletion cancelled");
            }
        }

        private void rescanConfigsButton_Click(object sender, RoutedEventArgs e)
        {
            vm.Rescan();
        }

        private void newConfigButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckSaved())
            {
                Globals.logger.LogWarning(Components.Stacker, "Config not saved, prompting quit confirmation");
                if (MessageBox.Show("The Config in Stacker wasn't saved.\r\nAre you sure you want to create a new config?",
                    "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                    return;
            }

            (new MainDialog(new DialogNewConfig(this), "New Config")).ShowDialog();
        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            vm.SearchString = filterTextbox.Text;
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
            catch { Globals.logger.LogError(Components.ConfigManager, "No config folder found!", true); }
        }
        #endregion

        #region Saving, Loading and Creating
        public void SaveConfig()
        {
            if (vm.CurrentConfig == null ||
                Globals.mainWindow.ConfigsPage.StackerPage == null ||
                Globals.mainWindow.ConfigsPage.OtherOptionsPage == null)
            {
                Globals.logger.LogError(Components.ConfigManager, "No config eligible for saving!", true);
                return;
            }

            if (vm.CurrentConfig.Remote)
            {
                Globals.logger.LogError(Components.ConfigManager, "The config was pulled from a remote source and cannot be saved!", true);
                return;
            }

            if (vm.CurrentConfigName == "") {
                Globals.logger.LogError(Components.ConfigManager, "Empty config name, cannot save", true);
                return;
            }

            var stacker = Globals.mainWindow.ConfigsPage.StackerPage.vm;
            stacker.ConvertKeychains();

            if (stacker.View == StackerView.Blocks)
                stacker.LS.FromBlocks(stacker.GetList());

            vm.CurrentConfig.Config.Script = stacker.LS.Script;

            Globals.logger.LogInfo(Components.ConfigManager, $"Saving config {vm.CurrentConfigName}");

            vm.CurrentConfig.Config.Settings.LastModified = DateTime.Now;
            vm.CurrentConfig.Config.Settings.Version = Globals.obVersion;
            Globals.logger.LogInfo(Components.ConfigManager, "Converted the unbinded observables and set the Last Modified date");

            // Save to file
            try
            {
                vm.SaveCurrent();

                // Save the last state of the config
                SaveState();
            }
            catch (Exception ex)
            {
                Globals.logger.LogError(Components.ConfigManager, $"Failed to save the config. Reason: {ex.Message}", true);
            }
        }

        public void LoadConfig(ConfigViewModel config)
        {
            if (config == null)
            {
                Globals.logger.LogError(Components.ConfigManager, "The config to load cannot be null");
            }

            // Set the config as current
            vm.CurrentConfig = config;

            if (vm.CurrentConfig.Remote)
            {
                Globals.logger.LogError(Components.ConfigManager, "The config was pulled from a remote source and cannot be edited!", true);
                vm.CurrentConfig = null;
                return;
            }

            Globals.logger.LogInfo(Components.ConfigManager, "Loading config: " + vm.CurrentConfig.Name);

            Globals.mainWindow.ConfigsPage.menuOptionStacker.IsEnabled = true;
            Globals.mainWindow.ConfigsPage.menuOptionOtherOptions.IsEnabled = true;

            var newStacker = new Stacker(vm.CurrentConfig);

            // Preserve the old stacker test data and proxy
            if (Globals.mainWindow.ConfigsPage.StackerPage != null)
            {
                newStacker.vm.TestData = Globals.mainWindow.ConfigsPage.StackerPage.vm.TestData;
                newStacker.vm.TestProxy = Globals.mainWindow.ConfigsPage.StackerPage.vm.TestProxy;
                newStacker.vm.ProxyType = Globals.mainWindow.ConfigsPage.StackerPage.vm.ProxyType;
            }

            Globals.mainWindow.ConfigsPage.StackerPage = newStacker; // Create a Stacker instance
            Globals.logger.LogInfo(Components.ConfigManager, "Created and assigned a new Stacker instance");
            Globals.mainWindow.ConfigsPage.OtherOptionsPage = new ConfigOtherOptions(); // Create an Other Options instance
            Globals.logger.LogInfo(Components.ConfigManager, "Created and assigned a new Other Options instance");
            Globals.mainWindow.ConfigsPage.menuOptionStacker_MouseDown(this, null); // Switch to Stacker

            // Save the last state of the config
            Globals.mainWindow.ConfigsPage.StackerPage.SetScript();
            SaveState();
        }

        public void CreateConfig(string name, string category, string author)
        {
            // Build the base config structure
            var settings = new ConfigSettings();
            settings.Name = name;
            settings.Author = author;
            
            var newConfig = new ConfigViewModel(name, category, new Config(settings, string.Empty));

            // Add it to the collection and persistent storage
            vm.Add(newConfig);

            vm.CurrentConfig = newConfig;
            var newStacker = new Stacker(vm.CurrentConfig);
            if (Globals.mainWindow.ConfigsPage.StackerPage != null) // Maintain the previous stacker settings
            {
                newStacker.vm.TestData = Globals.mainWindow.ConfigsPage.StackerPage.vm.TestData;
                newStacker.vm.TestProxy = Globals.mainWindow.ConfigsPage.StackerPage.vm.TestProxy;
                newStacker.vm.ProxyType = Globals.mainWindow.ConfigsPage.StackerPage.vm.ProxyType;
            }
            Globals.mainWindow.ConfigsPage.StackerPage = newStacker; // Create a Stacker instance
            Globals.logger.LogInfo(Components.ConfigManager, "Created and assigned a new Stacker instance");
            Globals.mainWindow.ConfigsPage.OtherOptionsPage = new ConfigOtherOptions(); // Create an Other Options instance
            Globals.logger.LogInfo(Components.ConfigManager, "Created and assigned a new Other Options instance");
            Globals.mainWindow.ConfigsPage.menuOptionStacker_MouseDown(this, null); // Switch to Stacker
        }
        #endregion

        #region ListView
        private void configsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try { vm.HoveredConfig = ((ConfigViewModel)configsListView.SelectedItem).Config; } catch { }
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

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            loadConfigButton_Click(this, new RoutedEventArgs());
        }
        #endregion
    }
}
