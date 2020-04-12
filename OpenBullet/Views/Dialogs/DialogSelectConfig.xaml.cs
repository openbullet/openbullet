﻿using OpenBullet.ViewModels;
using OpenBullet.Views.Main.Runner;
using OpenBullet.Views.UserControls;
using RuriLib;
using RuriLib.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace OpenBullet
{
    /// <summary>
    /// Logica di interazione per DialogSelectConfig.xaml
    /// </summary>
    public partial class DialogSelectConfig : Page
    {
        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;
        object Caller { get; set; }
        ConfigManagerViewModel vm = null;

        public DialogSelectConfig(object caller)
        {
            Caller = caller;
            vm = OB.ConfigManager;
            DataContext = vm;

            InitializeComponent();
        }
        
        private void selectButton_Click(object sender, RoutedEventArgs e)
        {
            if (configsListView.SelectedItems.Count == 0) return;
            if(Caller.GetType() == typeof(Runner))
            {
                var config = ((ConfigViewModel)configsListView.SelectedItem).Config;
                var runner = Caller as Runner;
                
                if (OB.OBSettings.General.LiveConfigUpdates) runner.SetConfig(config);
                else runner.SetConfig(IOManager.CloneConfig(config));
            }
            else if (Caller.GetType() == typeof(UserControlConfig))
            {
                ((UserControlConfig)Caller).Config = (ConfigViewModel)configsListView.SelectedItem;
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
            selectButton_Click(this, null);
        }

        private void ListViewItem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                selectButton_Click(this, null);
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
    }
}
