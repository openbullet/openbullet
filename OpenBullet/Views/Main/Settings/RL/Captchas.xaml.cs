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
            DataContext = Globals.rlSettings.Captchas;

            foreach (string i in Enum.GetNames(typeof(ServiceType)))
                currentServiceCombobox.Items.Add(i);

            currentServiceCombobox.SelectedIndex = (int)Globals.rlSettings.Captchas.CurrentService;
        }

        private void currentServiceCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Globals.rlSettings.Captchas.CurrentService = (ServiceType)currentServiceCombobox.SelectedIndex;
        }

        private void checkBalanceButton_Click(object sender, RoutedEventArgs e)
        {
            // Save
            IOManager.SaveSettings(Globals.rlSettingsFile, Globals.rlSettings);

            double balance = 0;

            try
            {
                balance = Service.Initialize(Globals.rlSettings.Captchas).GetBalance();
            }
            catch { balanceLabel.Content = "WRONG TOKEN / CREDENTIALS"; balanceLabel.Foreground = Globals.GetBrush("ForegroundBad"); return; }
            
            balanceLabel.Content = balance;
            balanceLabel.Foreground = balance > 0 ? Globals.GetBrush("ForegroundGood") : Globals.GetBrush("ForegroundBad");
        }
    }
}
