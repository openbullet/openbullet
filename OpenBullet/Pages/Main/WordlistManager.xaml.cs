using LiteDB;
using OpenBullet.ViewModels;
using RuriLib;
using RuriLib.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace OpenBullet
{
    /// <summary>
    /// Logica di interazione per WordlistManager.xaml
    /// </summary>
    public partial class WordlistManager : Page
    {
        WordlistManagerViewModel vm = new WordlistManagerViewModel();
        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;

        public WordlistManager()
        {
            InitializeComponent();
            DataContext = vm;

            vm.RefreshList();
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            (new MainDialog(new DialogAddWordlist(this), "Add Wordlist")).ShowDialog();
        }

        
        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            Globals.LogInfo(Components.WordlistManager, $"Deleting {wordlistListView.SelectedItems.Count} references from the DB");
            using (var db = new LiteDatabase(Globals.dataBaseFile))
            {
                var list = wordlistListView.SelectedItems.Cast<Wordlist>();
                while (list.Count() > 0)
                {
                    db.GetCollection<Wordlist>("wordlists").Delete(list.First().Id);
                    vm.WordlistList.Remove(list.First());
                }   
            }
            Globals.LogInfo(Components.WordlistManager, "Successfully deleted the wordlist references from the DB");
        }

        
        private void deleteAllButton_Click(object sender, RoutedEventArgs e)
        {
            Globals.LogWarning(Components.WordlistManager, "Purge selected, prompting warning");

            if (MessageBox.Show("This will purge the WHOLE Wordlists DB, are you sure you want to continue?", "WARNING", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                Globals.LogInfo(Components.WordlistManager, "Purge initiated");
                using (var db = new LiteDatabase(Globals.dataBaseFile))
                {
                    db.DropCollection("wordlists");
                }
                Globals.LogInfo(Components.WordlistManager, "Purge finished");
            }
            else { Globals.LogInfo(Components.WordlistManager, "Purge dismissed"); }

            vm.WordlistList.Clear();
        }

        public void AddWordlist(Wordlist wordlist)
        {
            vm.WordlistList.Add(wordlist);
            AddWordlistToDB(wordlist);
        }

        public void AddWordlistToDB(Wordlist wordlist)
        {
            // Add it to the DB
            Globals.LogInfo(Components.WordlistManager, "Adding Wordlist '" + wordlist.Name + " at position '" + wordlist.Path + "' with type '" + wordlist.Type + "' and purpose '" + wordlist.Purpose + "' to the DB");
            using (var db = new LiteDatabase(Globals.dataBaseFile))
            {
                db.GetCollection<Wordlist>("wordlists").Insert(wordlist);
            }
        }

        public List<Wordlist> GetLists()
        {
            return vm.WordlistList.ToList();
        }

        public Wordlist GetList(string name)
        {
            return vm.WordlistList.Where(x => x.Name == name).First();
        }

        private void listViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            string sortBy = column.Tag.ToString();
            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                wordlistListView.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            wordlistListView.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }

        private void wordlistListViewDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach(var file in files.Where(x => x.EndsWith(".txt")).ToArray())
                {
                    try
                    {
                        // Build the wordlist object
                        var wordlist = new Wordlist(Path.GetFileNameWithoutExtension(file), file, Globals.environment.WordlistTypes.First().Name, "");

                        // Get the first line
                        var first = File.ReadLines(wordlist.Path).First();

                        // Set the correct wordlist type
                        wordlist.Type = Globals.environment.RecognizeWordlistType(first);

                        // Add the wordlist to the manager
                        AddWordlist(wordlist);
                    }
                    catch { }
                }
            }
        }
    }
}
