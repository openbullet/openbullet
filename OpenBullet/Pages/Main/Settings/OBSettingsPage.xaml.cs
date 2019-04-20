using OpenBullet.ViewModels;
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

namespace OpenBullet.Pages.Main.Settings
{
    /// <summary>
    /// Logica di interazione per OBSettingsPage.xaml
    /// </summary>
    public partial class OBSettingsPage : Page
    {
        OBSettingsGeneral GeneralPage = new OBSettingsGeneral();
        OBSettingsSounds SoundsPage = new OBSettingsSounds();
        OBSettingsThemes ThemesPage = new OBSettingsThemes();

        public OBSettingsPage()
        {
            InitializeComponent();
            menuOptionGeneral_MouseDown(this, null);
        }

        #region Menu Options
        private void menuOptionGeneral_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Main.Content = GeneralPage;
            menuOptionSelected(menuOptionGeneral);
        }

        private void menuOptionSounds_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Main.Content = SoundsPage;
            menuOptionSelected(menuOptionSounds);
        }

        private void menuOptionThemes_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Main.Content = ThemesPage;
            menuOptionSelected(menuOptionThemes);
        }

        private void menuOptionSelected(object sender)
        {
            foreach (var child in topMenu.Children)
            {
                try
                {
                    var c = (Label)child;
                    c.Foreground = Globals.GetBrush("ForegroundMain");
                }
                catch { }
            }
            ((Label)sender).Foreground = Globals.GetBrush("ForegroundGood");
        }
#endregion

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            OBIOManager.SaveSettings(Globals.obSettingsFile, Globals.obSettings);
        }

        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to reset all your OpenBullet settings?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                Globals.obSettings.Reset();
        }
    }
}
