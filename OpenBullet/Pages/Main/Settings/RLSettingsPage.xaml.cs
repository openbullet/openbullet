using RuriLib;
using RuriLib.ViewModels;
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
    /// Logica di interazione per RLSettingsPage.xaml
    /// </summary>
    public partial class RLSettingsPage : Page
    {
        RLSettingsGeneral GeneralPage = new RLSettingsGeneral();
        RLSettingsProxies ProxiesPage = new RLSettingsProxies();
        RLSettingsCaptchas CaptchasPage = new RLSettingsCaptchas();
        RLSettingsSelenium SeleniumPage = new RLSettingsSelenium();

        public RLSettingsPage()
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

        private void menuOptionProxies_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Main.Content = ProxiesPage;
            menuOptionSelected(menuOptionProxies);
        }

        private void menuOptionCaptchas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Main.Content = CaptchasPage;
            menuOptionSelected(menuOptionCaptchas);
        }

        private void menuOptionSelenium_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Main.Content = SeleniumPage;
            menuOptionSelected(menuOptionSelenium);
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
            IOManager.SaveSettings(Globals.rlSettingsFile, Globals.rlSettings);
        }

        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to reset all your RuriLib settings?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                Globals.rlSettings.Reset();
        }
    }
}
