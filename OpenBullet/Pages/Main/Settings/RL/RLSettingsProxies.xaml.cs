using Extreme.Net;
using RuriLib.ViewModels;
using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace OpenBullet
{
    /// <summary>
    /// Logica di interazione per OBSettingsProxies.xaml
    /// </summary>
    public partial class RLSettingsProxies : Page
    {
        public RLSettingsProxies()
        {
            InitializeComponent();
            DataContext = Globals.rlSettings.Proxies;

            foreach (string i in Enum.GetNames(typeof(ProxyType)))
                if(i != "Chain") reloadTypeCombobox.Items.Add(i);

            reloadTypeCombobox.SelectedIndex = (int)Globals.rlSettings.Proxies.ReloadType;

            foreach (string s in Enum.GetNames(typeof(ProxyReloadSource)))
                reloadSourceCombobox.Items.Add(s);

            reloadSourceCombobox.SelectedIndex = (int)Globals.rlSettings.Proxies.ReloadSource;

            globalBanKeysTextbox.AppendText(string.Join(Environment.NewLine, Globals.rlSettings.Proxies.GlobalBanKeys), Colors.White);
            globalRetryKeysTextbox.AppendText(string.Join(Environment.NewLine, Globals.rlSettings.Proxies.GlobalRetryKeys), Colors.White);
        }
        
        private void globalBanKeysTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Globals.rlSettings.Proxies.GlobalBanKeys = globalBanKeysTextbox.Lines();
        }

        private void globalRetryKeysTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Globals.rlSettings.Proxies.GlobalRetryKeys = globalRetryKeysTextbox.Lines();
        }

        private void reloadSourceCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Globals.rlSettings.Proxies.ReloadSource = (ProxyReloadSource)reloadSourceCombobox.SelectedIndex;

            switch (Globals.rlSettings.Proxies.ReloadSource)
            {
                case ProxyReloadSource.Manager:
                    reloadTabControl.SelectedIndex = 0;
                    break;

                case ProxyReloadSource.API:
                case ProxyReloadSource.File:
                    reloadTabControl.SelectedIndex = 1;
                    break;
            }
        }

        private void reloadTypeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Globals.rlSettings.Proxies.ReloadType = (ProxyType)reloadTypeCombobox.SelectedIndex;
        }
    }
}
