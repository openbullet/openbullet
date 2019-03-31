using Newtonsoft.Json;
using RuriLib.CaptchaServices;
using RuriLib.LS;
using System.Windows.Media;

namespace RuriLib
{
    /// <summary>
    /// A block that can solve captcha challenges.
    /// </summary>
    public abstract class BlockCaptcha : BlockBase
    {
        /// <summary>
        /// The available Captcha-solving services.
        /// </summary>
        public enum CaptchaService
        {
            /// <summary>The service provided by https://anti-captcha.com/</summary>
            AntiCaptcha,

            /// <summary>The service provided by https://azcaptcha.com/</summary>
            AZCaptcha,

            /// <summary>The service provided by https://captchas.io/</summary>
            CaptchasIO,

            /// <summary>The service provided by https://www.deathbycaptcha.com/</summary>
            DBC,

            /// <summary>The service provided by http://de-captcher.com/</summary>
            DeCaptcher,

            /// <summary>The service provided by http://www.imagetyperz.com/</summary>
            ImageTypers,

            /// <summary>The service provided by https://www.solverecaptcha.com/</summary>
            SolveRecaptcha,

            /// <summary>The service provided by https://2captcha.com/</summary>
            TwoCaptcha
        }

        /// <summary>The balance of the account of the captcha-solving service.</summary>
        [JsonIgnore]
        public double Balance { get; set; } = 0;

        /// <inheritdoc />
        public override void Process(BotData data)
        {
            base.Process(data);

            // If bypass balance check, skip this method.
            if (data.GlobalSettings.Captchas.BypassBalanceCheck) return;

            // Get balance. If balance is under a certain threshold, don't ask for captcha solve
            Balance = 0; // Reset it or the block will save it for future calls
            data.Log(new LogEntry("Checking balance...", Colors.White));
            switch (data.GlobalSettings.Captchas.CurrentService)
            {
                case CaptchaService.ImageTypers:
                    Balance = new ImageTyperz(data.GlobalSettings.Captchas.ImageTypToken, 0).GetBalance();
                    break;

                case CaptchaService.AntiCaptcha:
                    Balance = new AntiCaptcha(data.GlobalSettings.Captchas.AntiCapToken, 0).GetBalance();
                    break;

                case CaptchaService.DBC:
                    Balance = new DeathByCaptcha(data.GlobalSettings.Captchas.DBCUser, data.GlobalSettings.Captchas.DBCPass, 0).GetBalance();
                    break;

                case CaptchaService.TwoCaptcha:
                    Balance = new TwoCaptcha(data.GlobalSettings.Captchas.TwoCapToken, 0).GetBalance();
                    break;

                case CaptchaService.DeCaptcher:
                    Balance = new DeCaptcher(data.GlobalSettings.Captchas.DCUser, data.GlobalSettings.Captchas.DCPass, 0).GetBalance();
                    break;

                case CaptchaService.AZCaptcha:
                    Balance = new AZCaptcha(data.GlobalSettings.Captchas.AZCapToken, 0).GetBalance();
                    break;

                case CaptchaService.SolveRecaptcha:
                    Balance = new SolveReCaptcha(data.GlobalSettings.Captchas.SRUserId, data.GlobalSettings.Captchas.SRToken, 0).GetBalance();
                    break;

                case CaptchaService.CaptchasIO:
                    Balance = new CaptchasIO(data.GlobalSettings.Captchas.CIOToken, 0).GetBalance();
                    break;

                default:
                    Balance = 999;
                    break;
            }

            if (Balance <= 0) throw new System.Exception($"[{data.GlobalSettings.Captchas.CurrentService}] Bad token/credentials or zero balance!");

            data.Log(new LogEntry($"[{data.GlobalSettings.Captchas.CurrentService}] Current Balance: ${Balance}", Colors.GreenYellow));
            data.Balance = Balance;
        }
    }
}
