using LiteDB;
using Microsoft.Win32;
using OpenBullet.ViewModels;
using RuriLib;
using RuriLib.Models;
using RuriLib.Runner;
using System;
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
    /// Logica di interazione per HitsDB.xaml
    /// </summary>
    public partial class HitsDB : Page
    {
        public HitsDBViewModel vm = new HitsDBViewModel();
        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;

        public HitsDB()
        {
            InitializeComponent();

            DataContext = vm;

            vm.RefreshList();

            var defaults = new string[] { "SUCCESS", "NONE" };
            foreach (string i in defaults.Concat(Globals.environment.GetCustomKeychainNames()))
                typeFilterCombobox.Items.Add(i);

            typeFilterCombobox.SelectedIndex = 0;

            configFilterCombobox.Items.Add("All");
            foreach (string c in vm.ConfigsList)
                configFilterCombobox.Items.Add(c);

            configFilterCombobox.SelectedIndex = 0;

            var menu = (ContextMenu)Resources["ItemContextMenu"];
            var copyMenu = (MenuItem)menu.Items[0];
            var saveMenu = (MenuItem)menu.Items[1];
            foreach (var f in Globals.environment.ExportFormats)
            {
                MenuItem i = new MenuItem();
                i.Header = f.Format;
                i.Click += new RoutedEventHandler(copySelectedCustom_Click);
                ((MenuItem)copyMenu.Items[4]).Items.Add(i); // Here the 4 is hardcoded, it's bad but it works
            }

            foreach (var f in Globals.environment.ExportFormats)
            {
                MenuItem i = new MenuItem();
                i.Header = f.Format;
                i.Click += new RoutedEventHandler(saveSelectedCustom_Click);
                ((MenuItem)saveMenu.Items[3]).Items.Add(i); // Here the 3 is hardcoded, it's bad but it works
            }
        }

        public void AddConfigToFilter(string name)
        {
            var alreadyThere = false;
            foreach(string item in configFilterCombobox.Items)
            {
                if (item == name) alreadyThere = true;
            }

            if (!alreadyThere) configFilterCombobox.Items.Add(name);
        }

        private void configFilterCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                vm.ConfigFilter = configFilterCombobox.SelectedValue.ToString();
            }
            catch { }
            Globals.LogInfo(Components.HitsDB, "Changed config filter to "+vm.ConfigFilter+", found "+vm.HitsList.Count+" hits");
        }

        private void typeFilterCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.TypeFilter = (string)typeFilterCombobox.SelectedValue;
            Globals.LogInfo(Components.HitsDB, $"Changed type filter to {vm.TypeFilter}, found {vm.HitsList.Count} hits");
        }

        private void purgeButton_Click(object sender, RoutedEventArgs e)
        {
            Globals.LogWarning(Components.HitsDB, "Purge selected, prompting warning");

            if (MessageBox.Show("This will purge the WHOLE Hits DB, are you sure you want to continue?", "WARNING", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                Globals.LogInfo(Components.HitsDB, "Purge initiated");

                using (var db = new LiteDatabase(Globals.dataBaseFile))
                {
                    db.DropCollection("hits");
                }

                vm.HitsList.Clear();

                Globals.LogInfo(Components.HitsDB, "Purge finished");
            }
            else { Globals.LogInfo(Components.HitsDB, "Purge dismissed"); }
        }

        private void listViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            string sortBy = column.Tag.ToString();
            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                hitsListView.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            hitsListView.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }

        private void ListViewItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private string GetSaveFile()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "TXT files | *.txt";
            sfd.FilterIndex = 1;
            sfd.ShowDialog();
            return sfd.FileName;
        }

        private void copySelectedData_Click(object sender, RoutedEventArgs e)
        {
            var clipboardText = "";
            try
            {
                foreach (Hit selected in hitsListView.SelectedItems)
                    clipboardText += selected.Data + Environment.NewLine;

                Globals.LogInfo(Components.HitsDB, $"Copied {hitsListView.SelectedItems.Count} hits");
                Clipboard.SetText(clipboardText);
            }
            catch (Exception ex) { Globals.LogError(Components.HitsDB, $"Exception while copying hits - {ex.Message}"); }
        }

        private void saveSelectedData_Click(object sender, RoutedEventArgs e)
        {
            var file = GetSaveFile();
            if (file == "") return;

            try
            {
                StreamWriter SaveFile = new StreamWriter(file);
                foreach (Hit selected in hitsListView.SelectedItems)
                {
                    SaveFile.WriteLine(selected.Data);
                }

                SaveFile.Close();
                
                Globals.LogInfo(Components.HitsDB, $"Saved {hitsListView.SelectedItems.Count} hits");
            }
            catch (Exception ex) { Globals.LogError(Components.HitsDB, $"Exception while saving hits - {ex.Message}"); }
        }

        private void copySelectedCapture_Click(object sender, RoutedEventArgs e)
        {
            var clipboardText = "";
            try
            {
                foreach (Hit selected in hitsListView.SelectedItems)
                    clipboardText += selected.Data + " | " + selected.CapturedData.ToCaptureString() + Environment.NewLine;

                Globals.LogInfo(Components.HitsDB, $"Copied {hitsListView.SelectedItems.Count} hits with capture");
                Clipboard.SetText(clipboardText);
            }
            catch (Exception ex) { Globals.LogError(Components.HitsDB, $"Exception while copying hits - {ex.Message}"); }
        }

        private void saveSelectedCapture_Click(object sender, RoutedEventArgs e)
        {
            var file = GetSaveFile();
            if (file == "") return;

            try
            {
                StreamWriter SaveFile = new StreamWriter(file);
                foreach (Hit selected in hitsListView.SelectedItems)
                {
                    SaveFile.WriteLine(selected.Data + " | " + selected.CapturedData.ToCaptureString());
                }

                SaveFile.Close();

                Globals.LogInfo(Components.HitsDB, $"Saved {hitsListView.SelectedItems.Count} hits");
            }
            catch (Exception ex) { Globals.LogError(Components.HitsDB, $"Exception while saving hits - {ex.Message}"); }
        }

        private void selectAll_Click(object sender, RoutedEventArgs e)
        {
            hitsListView.SelectAll();
        }

        private void copySelectedFull_Click(object sender, RoutedEventArgs e)
        {
            var clipboardText = "";
            try
            {
                foreach (Hit selected in hitsListView.SelectedItems)
                    clipboardText +=
                            "Data = " + selected.Data +
                            " | Type = " + selected.Type +
                            " | Config = " + selected.ConfigName +
                            " | Wordlist = " + selected.WordlistName +
                            " | Proxy = " + selected.Proxy +
                            " | Date = " + selected.Date.ToLongDateString() +
                            " | CapturedData = " + selected.CapturedData.ToCaptureString() +
                            Environment.NewLine;

                Globals.LogInfo(Components.HitsDB, $"Copied {hitsListView.SelectedItems.Count} hits (full)");
                Clipboard.SetText(clipboardText);
            }
            catch (Exception ex) { Globals.LogError(Components.HitsDB, $"Exception while copying hits - {ex.Message}"); }
        }

        private void saveSelectedFull_Click(object sender, RoutedEventArgs e)
        {
            var file = GetSaveFile();
            if (file == "") return;

            try
            {
                StreamWriter SaveFile = new StreamWriter(file);
                foreach (Hit selected in hitsListView.SelectedItems)
                {
                    SaveFile.WriteLine(
                            "Data = " + selected.Data +
                            " | Type = " + selected.Type +
                            " | Config = " + selected.ConfigName +
                            " | Wordlist = " + selected.WordlistName +
                            " | Proxy = " + selected.Proxy +
                            " | Date = " + selected.Date.ToLongDateString() +
                            " | CapturedData = " + selected.CapturedData.ToCaptureString()
                            );
                }

                SaveFile.Close();

                Globals.LogInfo(Components.HitsDB, $"Saved {hitsListView.SelectedItems.Count} hits");
            }
            catch (Exception ex) { Globals.LogError(Components.HitsDB, $"Exception while saving hits - {ex.Message}"); }
        }

        private void copySelectedCustom_Click(object sender, RoutedEventArgs e)
        {
            var clipboardText = "";
            try
            {
                foreach (Hit selected in hitsListView.SelectedItems)
                    clipboardText += selected.ToFormattedString((sender as MenuItem).Header.ToString().Replace(@"\r\n", "\r\n")) + Environment.NewLine;

                Globals.LogInfo(Components.HitsDB, $"Copied {hitsListView.SelectedItems.Count} hits (full)");
                Clipboard.SetText(clipboardText);
            }
            catch (Exception ex) { Globals.LogError(Components.HitsDB, $"Exception while copying hits - {ex.Message}"); }
        }

        private void saveSelectedCustom_Click(object sender, RoutedEventArgs e)
        {
            var file = GetSaveFile();
            if (file == "") return;

            try
            {
                StreamWriter SaveFile = new StreamWriter(file);
                foreach (Hit selected in hitsListView.SelectedItems)
                    SaveFile.WriteLine(selected.ToFormattedString((sender as MenuItem).Header.ToString().Replace(@"\r\n", "\r\n")) + Environment.NewLine);
                
                SaveFile.Close();

                Globals.LogInfo(Components.HitsDB, $"Saved {hitsListView.SelectedItems.Count} hits");
            }
            catch (Exception ex) { Globals.LogError(Components.HitsDB, $"Exception while saving hits - {ex.Message}"); }
        }

        private void copySelectedProxy_Click(object sender, RoutedEventArgs e)
        {
            try {
                var hit = (Hit)hitsListView.SelectedItem;
                Clipboard.SetText(hit.Proxy);
                Globals.LogInfo(Components.HitsDB, $"Copied the selected proxy {hit.Proxy}");
            } catch (Exception ex) { Globals.LogError(Components.HitsDB, $"Failed to copy selected proxy - {ex.Message}"); }
        }

        
        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            vm.SearchString = searchBar.Text;
            Globals.LogInfo(Components.HitsDB, "Changed capture filter to '"+ vm.SearchString + $"', found {vm.HitsList.Count} hits");
        }

        private void sendToRecheck_Click(object sender, RoutedEventArgs e)
        {
            if (hitsListView.SelectedItems.Count == 0) { Globals.LogError(Components.HitsDB, "No hits selected!", true); return; }
            var first = (Hit)hitsListView.SelectedItem;
            var partialName = "Recheck-" + BlockBase.MakeValidFileName(first.ConfigName);

            // Choose list name
            var fileName = BlockBase.GetFirstAvailableFileName("Wordlists\\", partialName, "txt");

            // Write to disk
            var path = $@"{Directory.GetCurrentDirectory()}\Wordlists\{fileName}";
            using (var sw = new StreamWriter(path))
            {
                foreach (Hit selected in hitsListView.SelectedItems)
                    sw.WriteLine(selected.Data);
            }

            // Import it
            var listName = fileName.Substring(0, fileName.Length - 4);
            var wordlist = new Wordlist(listName, path, Globals.environment.RecognizeWordlistType(first.Data), "");
            Globals.mainWindow.WordlistManagerPage.AddWordlist(wordlist);

            var runner = Globals.mainWindow.CurrentRunnerPage;
            if (runner == null)
            {
                MessageBox.Show("Please select a Runner from the manager first!");
                return;
            }
            else if (runner.vm.Busy)
            {
                MessageBox.Show("The selected Runner is busy, please choose another one!");
                return;
            }

            try
            {
                runner.vm.SetWordlist(Globals.mainWindow.WordlistManagerPage.GetList(listName));
            }
            catch { }

            // Try to select the config referring to the first selected hit
            try
            {
                runner.vm.SetConfig(Globals.mainWindow.ConfigsPage.ConfigManagerPage.vm.ConfigsList.Where(c => c.Name == first.ConfigName).First().Config,
                    Globals.obSettings.General.RecommendedBots);
            }
            catch { }

            // Switch to Runner
            Globals.mainWindow.menuOptionRunner_MouseDown(this, null);
        }

        private void deleteSelected_Click(object sender, RoutedEventArgs e)
        {
            Globals.LogInfo(Components.HitsDB, $"Deleting {hitsListView.SelectedItems.Count} hits");

            using (var db = new LiteDatabase(Globals.dataBaseFile))
            {
                var list = hitsListView.SelectedItems.Cast<Hit>();
                while (list.Count() > 0)
                {
                    db.GetCollection<Hit>("hits").Delete(list.First().Id);
                    vm.HitsList.Remove(list.First());
                }
            }

            Globals.LogInfo(Components.HitsDB, "Succesfully sent the delete query and refreshed the list");
        }

        private void removeDuplicatesButton_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new LiteDatabase(Globals.dataBaseFile))
            {
                var coll = db.GetCollection<Hit>("hits");
                var groups = coll
                    .FindAll()
                    .GroupBy(h => GetHitChecksum(h))
                    .Where(g => g.Count() > 1)
                    .Select(g => g.OrderBy(h => h.Date).Reverse().Skip(1));

                Globals.LogInfo(Components.HitsDB, $"Deleting {groups.Select(g => g.Count()).Sum()} duplicate hits");

                foreach (var group in groups)
                {
                    var list = group.ToList();
                    while (list.Count() > 0)
                    {
                        db.GetCollection<Hit>("hits").Delete(list.First().Id);
                        vm.HitsList.Remove(list.First());
                        list.RemoveAt(0);
                    }
                }
            }
        }

        private string GetHitChecksum(Hit hit)
        {
            return BlockFunction.GetHash(hit.Data + hit.ConfigName + hit.WordlistName, BlockFunction.Hash.MD5);
        }
    }
}
