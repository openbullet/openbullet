using RuriLib.Enums;
using System.Collections.Generic;
using System.Reflection;

namespace RuriLib.ViewModels
{
    /// <summary>
    /// Provides captcha-related settings.
    /// </summary>
    public class SettingsCaptchas : ViewModelBase
    {
        private CaptchaServiceType currentService = CaptchaServiceType.TwoCaptcha;
        /// <summary>Which Captcha Service is currently selected for solving captcha challenges.</summary>
        public CaptchaServiceType CurrentService { get { return currentService; } set { currentService = value; OnPropertyChanged(); } }

        private string antiCapToken = "";
        /// <summary>The AntiCaptcha API Token.</summary>
        public string AntiCapToken { get { return antiCapToken; } set { antiCapToken = value; OnPropertyChanged(); } }

        private string imageTypToken = "";
        /// <summary>The ImageTyperz API Token.</summary>
        public string ImageTypToken { get { return imageTypToken; } set { imageTypToken = value; OnPropertyChanged(); } }

        private string dbcUser = "";
        /// <summary>The DeathByCaptcha Username.</summary>
        public string DBCUser { get { return dbcUser; } set { dbcUser = value; OnPropertyChanged(); } }

        private string dbcPass = "";
        /// <summary>The DeathByCaptcha Password.</summary>
        public string DBCPass { get { return dbcPass; } set { dbcPass = value; OnPropertyChanged(); } }

        private string twoCapToken = "";
        /// <summary>The TwoCaptcha API Token.</summary>
        public string TwoCapToken { get { return twoCapToken; } set { twoCapToken = value; OnPropertyChanged(); } }

        private string ruCapToken = "";
        /// <summary>The RuCaptcha API Token.</summary>
        public string RuCapToken { get { return ruCapToken; } set { ruCapToken = value; OnPropertyChanged(); } }

        private string dcUser = "";
        /// <summary>The DeCaptcher Username.</summary>
        public string DCUser { get { return dcUser; } set { dcUser = value; OnPropertyChanged(); } }

        private string dcPass = "";
        /// <summary>The DeCaptcher Password.</summary>
        public string DCPass { get { return dcPass; } set { dcPass = value; OnPropertyChanged(); } }

        private string azCapToken = "";
        /// <summary>The AZCaptcha API Token.</summary>
        public string AZCapToken { get { return azCapToken; } set { azCapToken = value; OnPropertyChanged(); } }

        private string scToken = "";
        /// <summary>The SolveCaptcha API Token.</summary>
        public string SCToken { get { return scToken; } set { scToken = value; OnPropertyChanged(); } }

        private string srUserId = "";
        /// <summary>The SolveReCaptcha User Id.</summary>
        public string SRUserId { get { return srUserId; } set { srUserId = value; OnPropertyChanged(); } }

        private string srToken = "";
        /// <summary>The SolveReCaptcha API Token.</summary>
        public string SRToken { get { return srToken; } set { srToken = value; OnPropertyChanged(); } }

        private string trueCapUser = "";
        /// <summary>The TrueCaptcha username.</summary>
        public string TrueCapUser { get { return trueCapUser; } set { trueCapUser = value; OnPropertyChanged(); } }

        private string trueCapToken = "";
        /// <summary>The TrueCaptcha API Token.</summary>
        public string TrueCapToken { get { return trueCapToken; } set { trueCapToken = value; OnPropertyChanged(); } }

        private string cIOToken = "";
        /// <summary>The CaptchasIO API Token.</summary>
        public string CIOToken { get { return cIOToken; } set { cIOToken = value; OnPropertyChanged(); } }

        private string cdToken = "";
        /// <summary>The CaptchaDecoder API Token.</summary>
        public string CDToken { get { return cdToken; } set { cdToken = value; OnPropertyChanged(); } }

        private string customTwoCapToken = "";
        /// <summary>The custom 2Captcha API Token.</summary>
        public string CustomTwoCapToken { get { return customTwoCapToken; } set { customTwoCapToken = value; OnPropertyChanged(); } }

        private string customTwoCapDomain = "example.com";
        /// <summary>The custom 2Captcha server's domain.</summary>
        public string CustomTwoCapDomain { get { return customTwoCapDomain; } set { customTwoCapDomain = value; OnPropertyChanged(); } }

        private int customTwoCapPort = 80;
        /// <summary>The custom 2Captcha server's port.</summary>
        public int CustomTwoCapPort { get { return customTwoCapPort; } set { customTwoCapPort = value; OnPropertyChanged(); } }

        private bool bypassBalanceCheck = false;
        /// <summary>Whether to bypass the balance check before solving a captcha challenge.</summary>
        public bool BypassBalanceCheck { get { return bypassBalanceCheck; } set { bypassBalanceCheck = value; OnPropertyChanged(); } }
        
        private int timeout = 120;
        /// <summary>The maximum amount of time to wait until a captcha challenge is solved.</summary>
        public int Timeout { get { return timeout; } set { timeout = value; OnPropertyChanged(); } }

        /// <summary>
        /// Resets the properties to their default value.
        /// </summary>
        public void Reset()
        {
            SettingsCaptchas def = new SettingsCaptchas();
            IList<PropertyInfo> props = new List<PropertyInfo>(typeof(SettingsCaptchas).GetProperties());

            foreach (PropertyInfo prop in props)
            {
                prop.SetValue(this, prop.GetValue(def, null));
            }
        }
    }
}
