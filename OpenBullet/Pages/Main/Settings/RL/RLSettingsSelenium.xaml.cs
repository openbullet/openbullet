using RuriLib;
using RuriLib.ViewModels;
using System;
using System.Linq;
using System.Windows.Controls;

namespace OpenBullet
{
    /// <summary>
    /// Logica di interazione per OBSettingsSelenium.xaml
    /// </summary>
    public partial class RLSettingsSelenium : Page
    {
        public RLSettingsSelenium()
        {
            InitializeComponent();
            DataContext = Globals.rlSettings.Selenium;

            foreach (string i in Enum.GetNames(typeof(BrowserType)))
                browserTypeCombobox.Items.Add(i);

            browserTypeCombobox.SelectedIndex = (int)Globals.rlSettings.Selenium.Browser;
            foreach (var ext in Globals.rlSettings.Selenium.ChromeExtensions)
                extensionsBox.AppendText(ext + Environment.NewLine);
        }

        private void browserTypeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Globals.rlSettings.Selenium.Browser = (BrowserType)browserTypeCombobox.SelectedIndex;
        }

        private void extensionsBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Globals.rlSettings.Selenium.ChromeExtensions = extensionsBox.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).ToList();
        }
    }
}
