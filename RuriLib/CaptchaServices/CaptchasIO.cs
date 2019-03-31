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
    /// Implements the API of https://captchas.io/
    /// </summary>
    public class CaptchasIO : CaptchaService
    {
        /// <summary>
        /// Creates a CaptchasIO client.
        /// </summary>
        /// <param name="apiKey">The CaptchasIO API key</param>
        /// <param name="timeout">The maximum time to wait for the challenge to be solved</param>
        public CaptchasIO(string apiKey, int timeout) : base(apiKey, timeout) { }

        /// <inheritdoc />
        public override double GetBalance()
        {
            using (CWebClient client = new CWebClient())
            {
                if (Timeout > 0) client.Timeout = Timeout;
                var response = client.DownloadString($"http://api.captchas.io/res.php?key={ApiKey}&action=getbalance&json=1");
                GenericResponse gbr = JsonConvert.DeserializeObject<GenericResponse>(response);
                if (gbr.status == 0) throw new Exception(gbr.request);
                return double.Parse(gbr.request, CultureInfo.InvariantCulture);
            }
        }

        /// <inheritdoc />
        public override string SolveRecaptcha(string siteKey, string siteUrl)
        {
            // Create task
            using (CWebClient client = new CWebClient())
            {
                if (Timeout > 0) client.Timeout = Timeout;
                var response = client.DownloadString($"http://api.captchas.io/in.php?key={ApiKey}&method=userrecaptcha&googlekey={siteKey}&pageurl={siteUrl}&json=1");
                GenericResponse ctr = JsonConvert.DeserializeObject<GenericResponse>(response);
                if (ctr.status == 0) throw new Exception(ctr.request);
                TaskId = ctr.request;
                Status = CaptchaStatus.Processing;

                // Check if task has been completed
                DateTime start = DateTime.Now;
                while (Status == CaptchaStatus.Processing && (DateTime.Now - start).TotalSeconds < Timeout)
                {
                    Thread.Sleep(5000);
                    response = client.DownloadString($"http://api.captchas.io/res.php?key={ApiKey}&action=get&id={TaskId}&json=1");
                    GenericResponse gtrr = JsonConvert.DeserializeObject<GenericResponse>(response);
                    if (gtrr.request.Contains("NOT_READY")) continue;
                    if (gtrr.status == 0) throw new Exception(gtrr.request);
                    Status = CaptchaStatus.Completed;
                    return gtrr.request;
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
                var response = client.UploadString("http://api.captchas.io.com/in.php",
                    $"key={ApiKey}&method=base64&regsense=1&body={GetBase64(bitmap, ImageFormat.Bmp)}&json=1");
                GenericResponse ctr = JsonConvert.DeserializeObject<GenericResponse>(response);
                if (ctr.status == 0) throw new Exception(ctr.request);
                TaskId = ctr.request;
                Status = CaptchaStatus.Processing;

                // Check if task has been completed
                DateTime start = DateTime.Now;
                while (Status == CaptchaStatus.Processing && (DateTime.Now - start).TotalSeconds < Timeout)
                {
                    Thread.Sleep(5000);
                    response = client.DownloadString($"http://api.captchas.io/res.php?key={ApiKey}&action=get&id={TaskId}&json=1");
                    GenericResponse gtrr = JsonConvert.DeserializeObject<GenericResponse>(response);
                    if (gtrr.request.Contains("NOT_READY")) continue;
                    if (gtrr.status == 0) throw new Exception(gtrr.request);
                    Status = CaptchaStatus.Completed;
                    return gtrr.request;
                }
                throw new TimeoutException();
            }
        }

#pragma warning disable 0414
#pragma warning disable 0649
        struct GenericResponse
        {
            public int? status;
            public string request;
        }
#pragma warning restore 0414
#pragma warning restore 0649
    }
}
