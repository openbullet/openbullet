using OpenBullet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OpenBullet
{
    /// <summary>
    /// Logica di interazione per OBSettingsSources.xaml
    /// </summary>
    public partial class OBSettingsSources : Page
    {
        ViewModels.OBSettingsSources vm;
        Random rand = new Random();

        public OBSettingsSources()
        {
            InitializeComponent();

            vm = Globals.obSettings.Sources;
            DataContext = vm;
        }

        private void authTypeCombobox_Loaded(object sender, RoutedEventArgs e)
        {
            var s = vm.GetSourceById((int)(sender as ComboBox).Tag);
            if (s.AuthInitialized)
                return;

            s.AuthInitialized = true;
            foreach (var t in Enum.GetNames(typeof(Source.AuthMode)))
                (sender as ComboBox).Items.Add(t);

            (sender as ComboBox).SelectedIndex = (int)s.Auth;
        }

        private void authTypeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.GetSourceById((int)(sender as ComboBox).Tag).Auth = (Source.AuthMode)(sender as ComboBox).SelectedIndex;
        }

        private void removeSourceButton_Click(object sender, RoutedEventArgs e)
        {
            vm.RemoveSourceById((int)(sender as Button).Tag);
        }

        private void clearSourcesButton_Click(object sender, RoutedEventArgs e)
        {
            vm.Sources.Clear();
        }

        private void addSourceButton_Click(object sender, RoutedEventArgs e)
        {
            vm.Sources.Add(new Source(rand.Next()));
        }
    }
}
