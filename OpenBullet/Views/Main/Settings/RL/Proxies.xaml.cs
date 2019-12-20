using Extreme.Net;
using OpenBullet.Views.Main.Runner;
using RuriLib.Models;
using RuriLib.Runner;
using RuriLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace OpenBullet.Views.Main.Settings.RL
{
    /// <summary>
    /// Logica di interazione per Proxies.xaml
    /// </summary>
    public partial class Proxies : Page
    {
        SettingsProxies vm;
        Random rand = new Random();

        public Proxies()
        {
            InitializeComponent();
            vm = Globals.rlSettings.Proxies;
            DataContext = vm;

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
                    addRemoteProxySourceButton.Visibility = System.Windows.Visibility.Collapsed;
                    clearRemoteProxySourcesButton.Visibility = System.Windows.Visibility.Collapsed;
                    testRemoteProxySourcesButton.Visibility = System.Windows.Visibility.Collapsed;
                    break;

                case ProxyReloadSource.File:
                    reloadTabControl.SelectedIndex = 1;
                    addRemoteProxySourceButton.Visibility = System.Windows.Visibility.Collapsed;
                    clearRemoteProxySourcesButton.Visibility = System.Windows.Visibility.Collapsed;
                    testRemoteProxySourcesButton.Visibility = System.Windows.Visibility.Collapsed;
                    break;

                case ProxyReloadSource.Remote:
                    reloadTabControl.SelectedIndex = 2;
                    addRemoteProxySourceButton.Visibility = System.Windows.Visibility.Visible;
                    clearRemoteProxySourcesButton.Visibility = System.Windows.Visibility.Visible;
                    testRemoteProxySourcesButton.Visibility = System.Windows.Visibility.Visible;
                    break;
            }
        }

        private void reloadTypeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Globals.rlSettings.Proxies.ReloadType = (ProxyType)reloadTypeCombobox.SelectedIndex;
        }

        private void remoteProxyTypeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.GetRemoteProxySourceById((int)(sender as ComboBox).Tag).Type = (ProxyType)(sender as ComboBox).SelectedIndex;
        }

        private void remoteProxyTypeCombobox_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var s = vm.GetRemoteProxySourceById((int)(sender as ComboBox).Tag);
            if (s.TypeInitialized)
                return;

            s.TypeInitialized = true;
            foreach (var t in Enum.GetNames(typeof(ProxyType)))
            {
                if (t != "Chain")
                {
                    (sender as ComboBox).Items.Add(t);
                }
            }

            (sender as ComboBox).SelectedIndex = (int)s.Type;
        }

        private void removeRemoteProxySourceButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            vm.RemoveRemoteProxySourceById((int)(sender as Button).Tag);
        }

        private void addRemoteProxySourceButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            vm.RemoteProxySources.Add(new RemoteProxySource(rand.Next()));
        }

        private void clearRemoteProxySourcesButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure?", "Warning", MessageBoxButton.YesNo) == MessageBoxResult.No) return;
            vm.RemoteProxySources.Clear();
        }

        private async void TestRemoteProxySourcesButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            List<string> prompt = new List<string>() { "Results:" };
            var tasks = vm.RemoteProxySources
                .Where(s => s.Active)
                .Select(s => RunnerViewModel.GetProxiesFromRemoteSourceAsync(s.Url, s.Type, s.Pattern, s.Output))
                .ToList();

            foreach(var result in await Task.WhenAll(tasks))
            {
                if (result.Successful)
                {
                    var first = "NONE";
                    if (result.Proxies.Count > 0) first = result.Proxies.First().Proxy;
                    prompt.Add($"[SUCCESS] {result.Url} - Got {result.Proxies.Count} proxies (first: {first})");
                }
                else
                {
                    prompt.Add($"[FAILURE] {result.Url} - {result.Error}");
                }
            }

            MessageBox.Show(string.Join(Environment.NewLine, prompt));
        }
    }
}
