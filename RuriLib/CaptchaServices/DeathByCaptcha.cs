using Extreme.Net;
using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.CaptchaServices
{
    /// <summary>
    /// Implements the API of https://www.deathbycaptcha.com/
    /// </summary>
    public class DeathByCaptcha : CaptchaService
    {
        /// <summary>
        /// Creates a DeathByCaptcha client.
        /// </summary>
        /// <param name="user">The DeathByCaptcha username</param>
        /// <param name="pass">The DeathByCaptcha password</param>
        /// <param name="timeout">The maximum time to wait for the challenge to be solved</param>
        public DeathByCaptcha(string user, string pass, int timeout) : base(user, pass, timeout) { }

        /// <inheritdoc />
        public override double GetBalance()
        {
            using (CWebClient client = new CWebClient())
            {
                if (Timeout > 0) client.Timeout = Timeout;
                client.Headers.Add(HttpRequestHeader.Accept, "application/json");
                client.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
                var response = client.UploadString("http://api.dbcapi.me/api/user", $"username={User}&password={Pass}");
                GetBalanceResponse gbr = JsonConvert.DeserializeObject<GetBalanceResponse>(response);
                if (gbr.status != 0) throw new Exception("Invalid Credentials");
                return gbr.balance / 100;
            }
        }

        /// <inheritdoc />
        public override string SolveRecaptcha(string siteKey, string siteUrl)
        {
            // Create task
            HttpRequest request = new HttpRequest();
            request.AddHeader(HttpHeader.Accept, "application/json");
            var content = new Extreme.Net.MultipartContent(BlockRequest.GenerateMultipartBoundary());
            content.Add(new Extreme.Net.StringContent(User), "username");
            content.Add(new Extreme.Net.StringContent(Pass), "password");
            content.Add(new Extreme.Net.StringContent("4"), "type");
            content.Add(new Extreme.Net.StringContent(JsonConvert.SerializeObject(new CreateRecaptchaTaskRequest(siteUrl, siteKey))), "token_params");
            var response = request.Post("http://api.dbcapi.me/api/captcha", content).ToString();
            var split = response.Split('&');
            var status = int.Parse(split[0].Split('=')[1]);
            var id = split[1].Split('=')[1];
            if (status == 255) throw new Exception(response);
            TaskId = id;
            Status = CaptchaStatus.Processing;

            // Check if task has been completed
            DateTime start = DateTime.Now;
            while (Status == CaptchaStatus.Processing && (DateTime.Now - start).TotalSeconds < Timeout)
            {
                Thread.Sleep(5000);
                HttpRequest gRequest = new HttpRequest();
                gRequest.AddHeader(HttpHeader.Accept, "application/json");
                HttpResponse gResponse = gRequest.Get($"http://api.dbcapi.me/api/captcha/{TaskId}");
                var resp = gResponse.ToString();
                CreateTaskResponse gtrr = JsonConvert.DeserializeObject<CreateTaskResponse>(resp);
                if (gtrr == null) continue;
                if (!gtrr.is_correct) throw new Exception("No answer could be found");
                if (gtrr.text != "")
                {
                    Status = CaptchaStatus.Completed;
                    return gtrr.text;
                }
            }
            throw new TimeoutException();
        }

        /// <inheritdoc />
        public override string SolveCaptcha(Bitmap bitmap)
        {
            // Create task
            HttpRequest request = new HttpRequest();
            request.AddHeader(HttpHeader.Accept, "application/json");
            Extreme.Net.MultipartContent content = new Extreme.Net.MultipartContent(BlockRequest.GenerateMultipartBoundary());
            content.Add(new Extreme.Net.StringContent(User), "username");
            content.Add(new Extreme.Net.StringContent(Pass), "password");
            content.Add(new Extreme.Net.StringContent($"base64:{GetBase64(bitmap, ImageFormat.Jpeg)}"), "captchafile");
            var response = request.Post("http://api.dbcapi.me/api/captcha", content).ToString();
            var split = response.Split('&');
            var status = int.Parse(split[0].Split('=')[1]);
            var id = split[1].Split('=')[1];
            if (status == 255) throw new Exception(response);
            TaskId = id;
            Status = CaptchaStatus.Processing;

            // Check if task has been completed
            DateTime start = DateTime.Now;
            while (Status == CaptchaStatus.Processing && (DateTime.Now - start).TotalSeconds < Timeout)
            {
                Thread.Sleep(5000);
                HttpRequest gRequest = new HttpRequest();
                gRequest.AddHeader(HttpHeader.Accept, "application/json");
                HttpResponse gResponse = gRequest.Get($"http://api.dbcapi.me/api/captcha/{TaskId}");
                var resp = gResponse.ToString();
                CreateTaskResponse gtrr = JsonConvert.DeserializeObject<CreateTaskResponse>(resp);
                if (gtrr == null) continue;
                if (!gtrr.is_correct) throw new Exception("No answer could be found");
                if (gtrr.text != "")
                {
                    Status = CaptchaStatus.Completed;
                    return gtrr.text;
                }
            }
            throw new TimeoutException();
        }
#pragma warning disable 0649
#pragma warning disable 0414
        class GetBalanceResponse
        {
            public int status;
            public double balance;
        }

        class CreateRecaptchaTaskRequest
        {
            public string googlekey;
            public string pageurl;
            public CreateRecaptchaTaskRequest(string url, string key) { pageurl = url; googlekey = key; }
        }

        class CreateCaptchaTaskRequest
        {
            public string clientKey;
            public CaptchaTask task;
            public CreateCaptchaTaskRequest(string apiKey, string base64) { clientKey = apiKey; task = new CaptchaTask(base64); }
        }

        class CaptchaTask
        {
            public string type = "ImageToTextTask";
            public string body;
            public CaptchaTask(string base64) { body = base64; }
        }

        class CreateTaskResponse
        {
            public int captcha;
            public int status;
            public bool is_correct;
            public string text;
            public string error;
        }

        class GetTaskResultRequest
        {
            public string clientKey;
            public int taskId;
            public GetTaskResultRequest(string apiKey, int id) { clientKey = apiKey; taskId = id; }
        }

        class GetTaskResultCaptchaResponse
        {
            public int errorId;
            public string errorCode;
            public string status;
            public CaptchaSolution solution;
        }

        class GetTaskResultRecaptchaResponse
        {
            public int errorId;
            public string errorCode;
            public string status;
            public RecaptchaSolution solution;
        }

        class RecaptchaSolution
        {
            public string gRecaptchaResponse;
        }

        class CaptchaSolution
        {
            public string text;
        }

#pragma warning restore 0414
#pragma warning restore 0649
    }
}
