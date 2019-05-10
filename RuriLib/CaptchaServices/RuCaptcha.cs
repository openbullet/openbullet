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
    /// Implements the API of https://rucaptcha.com
    /// </summary>
    public class RuCaptcha : CaptchaService
    {
        /// <summary>
        /// Creates a RuCaptcha client.
        /// </summary>
        /// <param name="apiKey">The RuCaptcha API key</param>
        /// <param name="timeout">The maximum time to wait for the challenge to be solved</param>
        public RuCaptcha(string apiKey, int timeout) : base(apiKey, timeout) { }

        /// <inheritdoc />
        public override double GetBalance()
        {
            using (CWebClient client = new CWebClient())
            {
                if (Timeout > 0) client.Timeout = Timeout;
                var response = client.DownloadString($"http://rucaptcha.com/res.php?key={ApiKey}&action=getbalance");
                return double.Parse(response, CultureInfo.InvariantCulture);
            }
        }

        /// <inheritdoc />
        public override string SolveRecaptcha(string siteKey, string siteUrl)
        {
            // Create task
            using (CWebClient client = new CWebClient())
            {
                if (Timeout > 0) client.Timeout = Timeout;
                var response = client.DownloadString($"http://rucaptcha.com/in.php?key={ApiKey}&method=userrecaptcha&googlekey={siteKey}&pageurl={siteUrl}");
                if (!response.StartsWith("OK")) throw new Exception(response);
                TaskId = response.Split('|')[1];
                Status = CaptchaStatus.Processing;

                // Check if task has been completed
                DateTime start = DateTime.Now;
                while (Status == CaptchaStatus.Processing && (DateTime.Now - start).TotalSeconds < Timeout)
                {
                    Thread.Sleep(5000);
                    response = client.DownloadString($"http://rucaptcha.com/res.php?key={ApiKey}&action=get&id={TaskId}");
                    if (response.Contains("NOT_READY")) continue;
                    if (response.Contains("ERROR")) throw new Exception(response);
                    Status = CaptchaStatus.Completed;
                    return response.Split('|')[1];
                }
                throw new TimeoutException();
            }
        }

        /// <inheritdoc />
        public override string SolveCaptcha(Bitmap bitmap)
        {
            // Create task
            using (CWebClient client = new CWebClient())
            {
                if (Timeout > 0) client.Timeout = Timeout;
                client.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
                var response = client.UploadString("http://rucaptcha.com/in.php",
                    $"key={ApiKey}&method=base64&regsense=1&body={EscapeLongString(GetBase64(bitmap, ImageFormat.Bmp))}");
                if (!response.StartsWith("OK")) throw new Exception(response);
                TaskId = response.Split('|')[1];
                Status = CaptchaStatus.Processing;

                // Check if task has been completed
                DateTime start = DateTime.Now;
                while (Status == CaptchaStatus.Processing && (DateTime.Now - start).TotalSeconds < Timeout)
                {
                    Thread.Sleep(5000);
                    response = client.DownloadString($"http://rucaptcha.com/res.php?key={ApiKey}&action=get&id={TaskId}");
                    if (response.Contains("NOT_READY")) continue;
                    if (response.Contains("ERROR")) throw new Exception(response);
                    Status = CaptchaStatus.Completed;
                    return response.Split('|')[1];
                }
                throw new TimeoutException();
            }
        }

#pragma warning disable 0414
#pragma warning disable 0649
        class GenericResponse
        {
            public int status;
            public string request;
        }
#pragma warning restore 0414
#pragma warning restore 0649
    }
}