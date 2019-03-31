using OpenBullet.Pages.Main.Settings;
using RuriLib;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace OpenBullet
{
    /// <summary>
    /// Logica di interazione per OBSettings.xaml
    /// </summary>
    public partial class Settings : Page
    {
        OBSettingsPage OBSettingsPage = new OBSettingsPage();
        RLSettingsPage RLSettingsPage = new RLSettingsPage();

        public Settings()
        {
            InitializeComponent();

            menuOptionRuriLib_MouseDown(this, null);
        }

        #region Menu Options MouseDown Events
        private void menuOptionRuriLib_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Main.Content = RLSettingsPage;
            menuOptionSelected(menuOptionRuriLib);
        }

        private void menuOptionOpenBullet_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Main.Content = OBSettingsPage;
            menuOptionSelected(menuOptionOpenBullet);
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
            ((Label)sender).Foreground = Globals.GetBrush("ForegroundCustom");
        }
        #endregion

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            IOManager.SaveSettings(Globals.rlSettingsFile, Globals.rlSettings);
        }
    }
}
