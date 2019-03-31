using RuriLib.Models;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

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
            DataContext = Globals.mainWindow.WordlistManagerPage.DataContext;
        }

        
        private void selectButton_Click(object sender, RoutedEventArgs e)
        {
            if (Caller.GetType() == typeof(Runner))
            {
                ((Runner)Caller).SetWordlist((Wordlist)wordlistsListView.SelectedItem);
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
    }
}
