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
            TwoCaptcha,

            /// <summary>The service provided by https://rucaptcha.com/</summary>
            RuCaptcha,

            /// <summary>The service provided by a custom server using the official 2Captcha API.</summary>
            CustomTwoCaptcha
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
            var cs = data.GlobalSettings.Captchas;
            switch (cs.CurrentService)
            {
                case CaptchaService.ImageTypers:
                    Balance = new ImageTyperz(cs.ImageTypToken, cs.Timeout).GetBalance();
                    break;

                case CaptchaService.AntiCaptcha:
                    Balance = new AntiCaptcha(cs.AntiCapToken, cs.Timeout).GetBalance();
                    break;

                case CaptchaService.DBC:
                    Balance = new DeathByCaptcha(cs.DBCUser, cs.DBCPass, cs.Timeout).GetBalance();
                    break;

                case CaptchaService.TwoCaptcha:
                    Balance = new TwoCaptcha(cs.TwoCapToken, cs.Timeout).GetBalance();
                    break;

                case CaptchaService.RuCaptcha:
                    Balance = new RuCaptcha(cs.RuCapToken, cs.Timeout).GetBalance();
                    break;

                case CaptchaService.DeCaptcher:
                    Balance = new DeCaptcher(cs.DCUser, cs.DCPass, cs.Timeout).GetBalance();
                    break;

                case CaptchaService.AZCaptcha:
                    Balance = new AZCaptcha(cs.AZCapToken, cs.Timeout).GetBalance();
                    break;

                case CaptchaService.SolveRecaptcha:
                    Balance = new SolveReCaptcha(cs.SRUserId, cs.SRToken, cs.Timeout).GetBalance();
                    break;

                case CaptchaService.CaptchasIO:
                    Balance = new CaptchasIO(cs.CIOToken, cs.Timeout).GetBalance();
                    break;

                case CaptchaService.CustomTwoCaptcha:
                    Balance = new CustomTwoCaptcha(cs.CustomTwoCapToken, cs.CustomTwoCapDomain, cs.CustomTwoCapPort, cs.Timeout).GetBalance();
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
