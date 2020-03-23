using OpenBullet.ViewModels;
using OpenBullet.Views.Main.Runner;
using OpenBullet.Views.UserControls;
using RuriLib.Models;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;

namespace OpenBullet
{
    /// <summary>
    /// Logica di interazione per DialogSelectWordlist.xaml
    /// </summary>
    public partial class DialogSelectWordlist : Page
    {
        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;
        object Caller { get; set; }

        public DialogSelectWordlist(object caller)
        {
            InitializeComponent();
            Caller = caller;
            DataContext = OB.WordlistManager;
        }
        
        private void selectButton_Click(object sender, RoutedEventArgs e)
        {
            if (Caller.GetType() == typeof(Runner))
            {
                ((Runner)Caller).SetWordlist((Wordlist)wordlistsListView.SelectedItem);
            }
            else if (Caller.GetType() == typeof(UserControlWordlist))
            {
                ((UserControlWordlist)Caller).Wordlist = (Wordlist)wordlistsListView.SelectedItem;
            }
            ((MainDialog)Parent).Close();
        }

        private void listViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            string sortBy = column.Tag.ToString();
            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                wordlistsListView.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            wordlistsListView.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }

        private void ListViewItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            selectButton_Click(this, null);
        }

        private void ListViewItem_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
                selectButton_Click(this, null);
        }

        private void importWordlistButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Wordlist file | *.txt";
            ofd.FilterIndex = 1;
            ofd.ShowDialog();
            try
            {
                // Add the wordlist to the runner
                ((Runner)Caller).SetWordlist(WordlistManagerViewModel.FileToWordlist(ofd.FileName));

                ((MainDialog)Parent).Close();
            }
            catch { }
        }
    }
}
