using RuriLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuriLib.CaptchaServices
{
    /// <summary>
    /// The available Captcha-solving services.
    /// </summary>
    public enum ServiceType
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

        /// <summary>The service provided by https://2captcha.com/</summary>
        TwoCaptcha,

        /// <summary>The service provided by https://rucaptcha.com/</summary>
        RuCaptcha,

        /// <summary>The service provided by a custom server using the official 2Captcha API.</summary>
        CustomTwoCaptcha
    }

    /// <summary>
    /// Provides methods to initialize a captcha service given its type and perform an action.
    /// </summary>
    public static class Service
    {
        /// <summary>
        /// Initializes a Captcha Service basing on its type and authentication credentials.
        /// </summary>
        /// <param name="cs">The Captcha Settings</param>
        /// <returns>The Captcha Service.</returns>
        public static CaptchaService Initialize(SettingsCaptchas cs)
        {
            switch (cs.CurrentService)
            {
                case ServiceType.ImageTypers:
                    return new ImageTyperz(cs.ImageTypToken, cs.Timeout);

                case ServiceType.AntiCaptcha:
                    return new AntiCaptcha(cs.AntiCapToken, cs.Timeout);

                case ServiceType.DBC:
                    return new DeathByCaptcha(cs.DBCUser, cs.DBCPass, cs.Timeout);

                case ServiceType.TwoCaptcha:
                    return new TwoCaptcha(cs.TwoCapToken, cs.Timeout);

                case ServiceType.RuCaptcha:
                    return new RuCaptcha(cs.RuCapToken, cs.Timeout);

                case ServiceType.DeCaptcher:
                    return new DeCaptcher(cs.DCUser, cs.DCPass, cs.Timeout);

                case ServiceType.AZCaptcha:
                    return new AZCaptcha(cs.AZCapToken, cs.Timeout);

                case ServiceType.CaptchasIO:
                    return new CaptchasIO(cs.CIOToken, cs.Timeout);

                case ServiceType.CustomTwoCaptcha:
                    return new CustomTwoCaptcha(cs.CustomTwoCapToken, cs.CustomTwoCapDomain, cs.CustomTwoCapPort, cs.Timeout);

                default:
                    throw new NotSupportedException("Service not supported");
            }
        }
    }
}
