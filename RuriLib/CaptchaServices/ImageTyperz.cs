using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Net;
using System.Threading;

namespace RuriLib.CaptchaServices
{
    /// <summary>
    /// Implements the API of http://www.imagetyperz.com/
    /// </summary>
    public class ImageTyperz : CaptchaService
    {
        /// <summary>
        /// Creates an ImageTyperz client.
        /// </summary>
        /// <param name="apiKey">The ImageTyperz API key</param>
        /// <param name="timeout">The maximum time to wait for the challenge to be solved</param>
        public ImageTyperz(string apiKey, int timeout) : base(apiKey, timeout) { }

        /// <inheritdoc />
        public override double GetBalance()
        {
            using (CWebClient client = new CWebClient())
            {
                if (Timeout > 0) client.Timeout = Timeout;
                client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                var res = client.UploadString("http://captchatypers.com/Forms/RequestBalanceToken.ashx", $"token={ApiKey}&action=REQUESTBALANCE");
                if (res.Contains("ERROR")) throw new Exception(res);
                return double.Parse(res, CultureInfo.InvariantCulture);
            }
        }

        /// <inheritdoc />
        public override string SolveRecaptcha(string siteKey, string siteUrl)
        {
            // Create task
            using (CWebClient client = new CWebClient())
            {
                if (Timeout > 0) client.Timeout = Timeout;
                client.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
                var res = client.UploadString("http://captchatypers.com/captchaapi/UploadRecaptchaToken.ashx",
                    $"token={ApiKey}&action=UPLOADCAPTCHA&pageurl={siteUrl}&googlekey={siteKey}");
                if (res.Contains("ERROR")) throw new Exception(res);
                TaskId = res;
                Status = CaptchaStatus.Processing;

                // Check if task has been completed
                DateTime start = DateTime.Now;
                while (Status == CaptchaStatus.Processing && (DateTime.Now - start).TotalSeconds < Timeout)
                {
                    Thread.Sleep(5000);
                    client.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
                    res = client.UploadString("http://captchatypers.com/captchaapi/GetRecaptchaTextToken.ashx",
                        $"token={ApiKey}&action=GETTEXT&captchaID={TaskId}");
                    if (res.Contains("NOTDECODED")) continue;
                    if (res.Contains("ERROR")) throw new Exception(res);
                    Status = CaptchaStatus.Completed;
                    return res;
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
                var res = client.UploadString("http://captchatypers.com/Forms/UploadFileAndGetTextNEWToken.ashx",
                    $"token={ApiKey}&action=UPLOADCAPTCHA&chkCase=1&file={WebUtility.UrlEncode(GetBase64(bitmap, ImageFormat.Bmp))}");
                if (res.Contains("ERROR")) throw new Exception(res);
                Status = CaptchaStatus.Completed;
                return res.Split('|')[1];
            }
        }
    }
}
