using RuriLib;
using RuriLib.CaptchaServices;
using System;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet
{
    /// <summary>
    /// Logica di interazione per OBSettingsCaptchas.xaml
    /// </summary>
    public partial class RLSettingsCaptchas : Page
    {
        public RLSettingsCaptchas()
        {
            InitializeComponent();
            DataContext = Globals.rlSettings.Captchas;

            foreach (string i in Enum.GetNames(typeof(BlockCaptcha.CaptchaService)))
                currentServiceCombobox.Items.Add(i);

            currentServiceCombobox.SelectedIndex = (int)Globals.rlSettings.Captchas.CurrentService;
        }

        private void currentServiceCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Globals.rlSettings.Captchas.CurrentService = (BlockCaptcha.CaptchaService)currentServiceCombobox.SelectedIndex;
        }

        private void checkBalanceButton_Click(object sender, RoutedEventArgs e)
        {
            // Save
            IOManager.SaveSettings(Globals.rlSettingsFile, Globals.rlSettings);

            double balance = 0;

            try
            {
                var cs = Globals.rlSettings.Captchas;
                switch (cs.CurrentService)
                {
                    case BlockCaptcha.CaptchaService.AntiCaptcha:
                        balance = new AntiCaptcha(cs.AntiCapToken, cs.Timeout).GetBalance();
                        break;

                    case BlockCaptcha.CaptchaService.DBC:
                        balance = new DeathByCaptcha(cs.DBCUser, cs.DBCPass, cs.Timeout).GetBalance();
                        break;

                    case BlockCaptcha.CaptchaService.DeCaptcher:
                        balance = new DeCaptcher(cs.DCUser, cs.DCPass, cs.Timeout).GetBalance();
                        break;

                    case BlockCaptcha.CaptchaService.ImageTypers:
                        balance = new ImageTyperz(cs.ImageTypToken, cs.Timeout).GetBalance();
                        break;

                    case BlockCaptcha.CaptchaService.TwoCaptcha:
                        balance = new TwoCaptcha(cs.TwoCapToken, cs.Timeout).GetBalance();
                        break;

                    case BlockCaptcha.CaptchaService.RuCaptcha:
                        balance = new RuCaptcha(cs.RuCapToken, cs.Timeout).GetBalance();
                        break;

                    case BlockCaptcha.CaptchaService.AZCaptcha:
                        balance = new AZCaptcha(cs.AZCapToken, cs.Timeout).GetBalance();
                        break;

                    case BlockCaptcha.CaptchaService.SolveRecaptcha:
                        balance = new SolveReCaptcha(cs.SRUserId, cs.SRToken, cs.Timeout).GetBalance();
                        break;

                    case BlockCaptcha.CaptchaService.CaptchasIO:
                        balance = new CaptchasIO(cs.CIOToken, cs.Timeout).GetBalance();
                        break;

                    case BlockCaptcha.CaptchaService.CustomTwoCaptcha:
                        balance = new CustomTwoCaptcha(cs.CustomTwoCapToken, cs.CustomTwoCapDomain, cs.CustomTwoCapPort, cs.Timeout).GetBalance();
                        break;

                    default:
                        balance = 999;
                        break;
                }
            }
            catch { balanceLabel.Content = "WRONG TOKEN / CREDENTIALS"; balanceLabel.Foreground = Globals.GetBrush("ForegroundBad"); return; }
            
            balanceLabel.Content = balance;
            balanceLabel.Foreground = balance > 0 ? Globals.GetBrush("ForegroundGood") : Globals.GetBrush("ForegroundBad");
        }
    }
}
