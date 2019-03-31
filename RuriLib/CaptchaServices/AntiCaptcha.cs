using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Threading;

namespace RuriLib.CaptchaServices
{
    /// <summary>
    /// Implements the API of https://anti-captcha.com/
    /// </summary>
    public class AntiCaptcha : CaptchaService
    {
        /// <summary>
        /// Creates an AntiCaptcha client.
        /// </summary>
        /// <param name="apiKey">The AntiCaptcha API key</param>
        /// <param name="timeout">The maximum time to wait for the challenge to be solved</param>
        public AntiCaptcha(string apiKey, int timeout) : base(apiKey, timeout) { }

        /// <inheritdoc />
        public override double GetBalance()
        {
            using (CWebClient client = new CWebClient())
            {
                if (Timeout > 0) client.Timeout = Timeout;
                client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                var response = client.UploadString("https://api.anti-captcha.com/getBalance",
                    JsonConvert.SerializeObject(new GetBalanceRequest(ApiKey)));
                GetBalanceResponse gbr = JsonConvert.DeserializeObject<GetBalanceResponse>(response);
                if (gbr.errorId > 0) throw new Exception(gbr.errorCode);
                return gbr.balance;
            }
        }

        /// <inheritdoc />
        public override string SolveRecaptcha(string siteKey, string siteUrl)
        {
            // Create task
            using (CWebClient client = new CWebClient())
            {
                if (Timeout > 0) client.Timeout = Timeout;
                client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                var response = client.UploadString("https://api.anti-captcha.com/createTask",
                    JsonConvert.SerializeObject(new CreateRecaptchaTaskRequest(ApiKey, siteUrl, siteKey)));
                CreateTaskResponse ctr = JsonConvert.DeserializeObject<CreateTaskResponse>(response);
                if (ctr.errorId > 0) throw new Exception(ctr.errorCode);
                TaskId = ctr.taskId;
                Status = CaptchaStatus.Processing;

                // Check if task has been completed
                DateTime start = DateTime.Now;
                while (Status == CaptchaStatus.Processing && (DateTime.Now - start).TotalSeconds < Timeout)
                {
                    Thread.Sleep(5000);
                    client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                    response = client.UploadString("https://api.anti-captcha.com/getTaskResult",
                        JsonConvert.SerializeObject(new GetTaskResultRequest(ApiKey, TaskId)));
                    GetTaskResultRecaptchaResponse gtrr = JsonConvert.DeserializeObject<GetTaskResultRecaptchaResponse>(response);
                    if (gtrr.errorId > 0) throw new Exception(gtrr.errorCode);
                    if (gtrr.status == "ready")
                    {
                        Status = CaptchaStatus.Completed;
                        return gtrr.solution.gRecaptchaResponse;
                    }
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
                client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                var response = client.UploadString("https://api.anti-captcha.com/createTask",
                    JsonConvert.SerializeObject(new CreateCaptchaTaskRequest(ApiKey, GetBase64(bitmap, ImageFormat.Png))));
                CreateTaskResponse ctr = JsonConvert.DeserializeObject<CreateTaskResponse>(response);
                if (ctr.errorId > 0) throw new Exception(ctr.errorCode);
                TaskId = ctr.taskId;
                Status = CaptchaStatus.Processing;

                // Check if task has been completed
                DateTime start = DateTime.Now;
                while (Status == CaptchaStatus.Processing && (DateTime.Now - start).TotalSeconds < Timeout)
                {
                    Thread.Sleep(5000);
                    client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                    response = client.UploadString("https://api.anti-captcha.com/getTaskResult",
                        JsonConvert.SerializeObject(new GetTaskResultRequest(ApiKey, TaskId)));
                    GetTaskResultCaptchaResponse gtrr = JsonConvert.DeserializeObject<GetTaskResultCaptchaResponse>(response);
                    if (gtrr.errorId > 0) throw new Exception(gtrr.errorCode);
                    if (gtrr.status == "ready")
                    {
                        Status = CaptchaStatus.Completed;
                        return gtrr.solution.text;
                    }
                }
                throw new TimeoutException();
            }
        }

#pragma warning disable 0414
#pragma warning disable 0649
        class GetBalanceRequest
        {
            public string clientKey;
            public GetBalanceRequest(string apiKey) { clientKey = apiKey; }
        }

        class GetBalanceResponse
        {
            public int errorId;
            public string errorCode;
            public double balance;
        }

        class CreateRecaptchaTaskRequest
        {
            public string clientKey;
            public RecaptchaTask task;
            public CreateRecaptchaTaskRequest(string apiKey, string url, string key) { clientKey = apiKey; task = new RecaptchaTask(url, key); }
        }

        class RecaptchaTask
        {
            public string type = "NoCaptchaTaskProxyless";
            public string websiteURL;
            public string websiteKey;
            public RecaptchaTask(string url, string key) { websiteURL = url; websiteKey = key; }
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
            public int errorId;
            public int taskId;
            public string errorCode;
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
