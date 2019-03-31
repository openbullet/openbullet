using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Net;
using System.Threading;

namespace RuriLib.CaptchaServices
{
    /// <summary>
    /// Implements the API of https://www.solverecaptcha.com/
    /// </summary>
    public class SolveReCaptcha : CaptchaService
    {
        /// <summary>
        /// Creates a SolveRecaptcha client.
        /// </summary>
        /// <param name="userId">The id of the user</param>
        /// <param name="apiKey">The SolveRecaptcha API key</param>
        /// <param name="timeout">The maximum time to wait for the challenge to be solved</param>
        public SolveReCaptcha(string userId, string apiKey, int timeout) : base(userId, apiKey, timeout) { }

        /// <inheritdoc />
        public override string SolveRecaptcha(string siteKey, string siteUrl)
        {
            // Create task
            using (CWebClient client = new CWebClient())
            {
                if (Timeout > 0) client.Timeout = Timeout;
                var response = client.DownloadString($"https://api.solverecaptcha.com/?userid={User}&key={Pass}&sitekey={siteKey}&pageurl={siteUrl}");
                if (response.Contains("ERROR")) throw new Exception(response);
                Status = CaptchaStatus.Completed;
                return response.Split('|')[1];
            }
        }

        /// <inheritdoc />
        public override double GetBalance()
        {
            // Create task
            using (CWebClient client = new CWebClient())
            {
                client.Timeout = 3;
                try
                {
                    var response = client.DownloadString($"https://api.solverecaptcha.com/?userid={User}&key={Pass}&sitekey=test&pageurl=test");
                    if (response.Contains("ERROR_ACCESS_DENIED")) return 0; // Invalid key
                }
                catch { } // Catch the timeout exception, this means it's trying to process a captcha so the key is valid
                return 999;
            }
        }
    }
}
