using RuriLib;
using RuriLib.CaptchaServices;
using System;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet.Views.Main.Settings.RL
{
    /// <summary>
    /// Logica di interazione per Captchas.xaml
    /// </summary>
    public partial class Captchas : Page
    {
        public Captchas()
        {
            InitializeComponent();
            DataContext = OB.Settings.RLSettings.Captchas;

            foreach (string i in Enum.GetNames(typeof(ServiceType)))
                currentServiceCombobox.Items.Add(i);

            currentServiceCombobox.SelectedIndex = (int)OB.Settings.RLSettings.Captchas.CurrentService;
        }

        private void currentServiceCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OB.Settings.RLSettings.Captchas.CurrentService = (ServiceType)currentServiceCombobox.SelectedIndex;
        }

        private void checkBalanceButton_Click(object sender, RoutedEventArgs e)
        {
            // Save
            IOManager.SaveSettings(OB.rlSettingsFile, OB.Settings.RLSettings);

            double balance = 0;

            try
            {
                balance = Service.Initialize(OB.Settings.RLSettings.Captchas).GetBalance();
            }
            catch { balanceLabel.Content = "WRONG TOKEN / CREDENTIALS"; balanceLabel.Foreground = Utils.GetBrush("ForegroundBad"); return; }
            
            balanceLabel.Content = balance;
            balanceLabel.Foreground = balance > 0 ? Utils.GetBrush("ForegroundGood") : Utils.GetBrush("ForegroundBad");
        }
    }
}
