using RuriLib.ViewModels;
using System;
using System.Linq;
using System.Windows.Controls;

namespace OpenBullet.Views.Main.Settings.RL
{
    /// <summary>
    /// Logica di interazione per Selenium.xaml
    /// </summary>
    public partial class Selenium : Page
    {
        public Selenium()
        {
            InitializeComponent();
            DataContext = OB.Settings.RLSettings.Selenium;

            foreach (string i in Enum.GetNames(typeof(BrowserType)))
                browserTypeCombobox.Items.Add(i);

            browserTypeCombobox.SelectedIndex = (int)OB.Settings.RLSettings.Selenium.Browser;
            foreach (var ext in OB.Settings.RLSettings.Selenium.ChromeExtensions)
                extensionsBox.AppendText(ext + Environment.NewLine);
        }

        private void browserTypeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OB.Settings.RLSettings.Selenium.Browser = (BrowserType)browserTypeCombobox.SelectedIndex;
        }

        private void extensionsBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            OB.Settings.RLSettings.Selenium.ChromeExtensions = extensionsBox.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).ToList();
        }
    }
}
