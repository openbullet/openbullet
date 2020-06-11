using CaptchaSharp;
using CaptchaSharp.Services;
using CaptchaSharp.Services.More;
using RuriLib.Enums;
using RuriLib.ViewModels;
using System;

namespace RuriLib.Functions.Captchas
{
    /// <summary>Provides methods for interacting with captcha solving services.</summary>
    public static class Captchas
    {
        /// <summary>Gets a <see cref="CaptchaService"/> to be used for solving captcha challenges.</summary>
        public static CaptchaService GetService(SettingsCaptchas settings)
        {
            CaptchaService service;

            switch (settings.CurrentService)
            {
                case CaptchaServiceType.AntiCaptcha:
                    service = new AntiCaptchaService(settings.AntiCapToken);
                    break;

                case CaptchaServiceType.AzCaptcha:
                    service = new AzCaptchaService(settings.AZCapToken);
                    break;

                case CaptchaServiceType.CaptchasIO:
                    service = new CaptchasIOService(settings.CIOToken);
                    break;

                case CaptchaServiceType.CustomTwoCaptcha:
                    service = new CustomTwoCaptchaService(settings.CustomTwoCapToken,
                        new Uri($"http://{settings.CustomTwoCapDomain}:{settings.CustomTwoCapPort}"));
                    break;

                case CaptchaServiceType.DeathByCaptcha:
                    service = new DeathByCaptchaService(settings.DBCUser, settings.DBCPass);
                    break;

                case CaptchaServiceType.DeCaptcher:
                    service = new DeCaptcherService(settings.DCUser, settings.DCPass);
                    break;

                case CaptchaServiceType.ImageTyperz:
                    service = new ImageTyperzService(settings.ImageTypToken);
                    break;

                case CaptchaServiceType.CapMonster:
                    service = new CapMonsterService(settings.CustomTwoCapToken,
                        new Uri($"http://{settings.CustomTwoCapDomain}:{settings.CustomTwoCapPort}"));
                    break;

                case CaptchaServiceType.RuCaptcha:
                    service = new RuCaptchaService(settings.RuCapToken);
                    break;

                case CaptchaServiceType.SolveCaptcha:
                    service = new SolveCaptchaService(settings.SCToken);
                    break;

                case CaptchaServiceType.SolveRecaptcha:
                    service = new SolveRecaptchaService(settings.SRToken);
                    break;

                case CaptchaServiceType.TrueCaptcha:
                    service = new TrueCaptchaService(settings.TrueCapUser, settings.TrueCapToken);
                    break;

                case CaptchaServiceType.TwoCaptcha:
                    service = new TwoCaptchaService(settings.TwoCapToken);
                    break;

                default:
                    throw new NotSupportedException();
            }

            service.Timeout = TimeSpan.FromSeconds(settings.Timeout);
            return service;
        }
    }
}
