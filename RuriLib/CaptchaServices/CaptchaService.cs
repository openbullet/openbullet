using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace RuriLib.CaptchaServices
{
    /// <summary>
    /// Represent a captcha-solving service.
    /// </summary>
    public abstract class CaptchaService
    {
        /// <summary>
        /// The busy status of the service.
        /// </summary>
        public enum CaptchaStatus
        {
            /// <summary>No captcha is being solved.</summary>
            Idle,

            /// <summary>A captcha is being solved.</summary>
            Processing,

            /// <summary>A captcha was solved.</summary>
            Completed
        }

        /// <summary>The API key to access the captcha service.</summary>        
        public string ApiKey { get; set; }

        /// <summary>The username to access the captcha service.</summary>
        public string User { get; set; }

        /// <summary>The password to access the captcha service.</summary>
        public string Pass { get; set; }

        /// <summary>The maximum time to wait for captcha completion.</summary>
        public int Timeout { get; set; }

        /// <summary>The id of the captcha-solving task being performed on the remote server, used to periodically query the status of the task.</summary>
        public dynamic TaskId { get; set; }

        /// <summary>The status of the captcha service.</summary>
        public CaptchaStatus Status { get; set; }

        /// <summary>
        /// Creates a captcha service that authenticates via API key.
        /// </summary>
        /// <param name="apiKey">The API key of the account</param>
        /// <param name="timeout">The maximum time to wait for captcha completion</param>
        public CaptchaService(string apiKey, int timeout)
        {
            ApiKey = apiKey;
            Timeout = timeout;
            Status = CaptchaStatus.Idle;
        }

        /// <summary>
        /// Creates a captcha service that authenticates via username and password.
        /// </summary>
        /// <param name="user">The username of the account</param>
        /// <param name="pass">The password of the account</param>
        /// <param name="timeout">The maximum time to wait for captcha completion</param>
        public CaptchaService(string user, string pass, int timeout)
        {
            User = user;
            Pass = pass;
            Timeout = timeout;
            Status = CaptchaStatus.Idle;
        }

        /// <summary>
        /// Gets the remaining balance in the account.
        /// </summary>
        /// <returns>The balance of the account</returns>
        public virtual double GetBalance()
        {
            return 0;
        }

        /// <summary>
        /// Solves a reCaptcha challenge for a given site.
        /// </summary>
        /// <param name="siteKey">The SiteKey found in the page's source code</param>
        /// <param name="siteUrl">The URL of the site to solve a reCaptcha for</param>
        /// <returns>The g-recaptcha-response of the challenge</returns>
        public virtual string SolveRecaptcha(string siteKey, string siteUrl)
        {
            return string.Empty;
        }

        /// <summary>
        /// Solves an image captcha challenge.
        /// </summary>
        /// <param name="image">The bitmap image of the challenge</param>
        /// <returns>The text written in the image</returns>
        public virtual string SolveCaptcha(Bitmap image)
        {
            return string.Empty;
        }

        /// <summary>
        /// Gets the base64-encoded string from an image.
        /// </summary>
        /// <param name="image">The bitmap image</param>
        /// <param name="format">The format of the bitmap image</param>
        /// <returns>The base64-encoded string</returns>
        protected string GetBase64(Bitmap image, ImageFormat format)
        {
            MemoryStream stream = new MemoryStream();
            image.Save(stream, format);
            byte[] imageBytes = stream.ToArray();
            return Convert.ToBase64String(imageBytes);
        }

        /// <summary>
        /// Gets a memory stream from an image.
        /// </summary>
        /// <param name="image">The bitmap image</param>
        /// <param name="format">The format of the bitmap image</param>
        /// <returns>The memory stream</returns>
        protected Stream GetStream(Bitmap image, ImageFormat format)
        {
            Stream stream = new MemoryStream();
            image.Save(stream, format);
            return stream;
        }

        /// <summary>
        /// Gets the bytes from an image.
        /// </summary>
        /// <param name="image">The bitmap image</param>
        /// <returns>The bytes of the image</returns>
        protected byte[] GetBytes(Bitmap image)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(image, typeof(byte[]));
        }

        /// <summary>
        /// Performs a synchronous POST request that returns a string.
        /// </summary>
        /// <param name="client">The HttpClient that performs the async request</param>
        /// <param name="url">The URL to call</param>
        /// <param name="content">The content that needs to be posted</param>
        /// <returns>The response as a string</returns>
        protected string PostSync(HttpClient client, string url, HttpContent content)
        {
            return Task.Run(() =>
                       Task.Run(() =>
                           client.PostAsync(url, content))
                       .Result.Content.ReadAsStringAsync())
                   .Result;
        }

        /// <summary>
        /// Performs a synchronous POST request that returns a byte array.
        /// </summary>
        /// <param name="client">The HttpClient that performs the async request</param>
        /// <param name="url">The URL to call</param>
        /// <param name="content">The content that needs to be posted</param>
        /// <returns>The response as a byte array</returns>
        protected byte[] PostSync2(HttpClient client, string url, HttpContent content)
        {
            return Task.Run(() =>
                       Task.Run(() =>
                           client.PostAsync(url, content))
                       .Result.Content.ReadAsByteArrayAsync())
                   .Result;
        }

        /// <summary>
        /// Performs a synchronous POST request that returns a stream.
        /// </summary>
        /// <param name="client">The HttpClient that performs the async request</param>
        /// <param name="url">The URL to call</param>
        /// <param name="content">The content that needs to be posted</param>
        /// <returns>The response as a stream</returns>
        protected Stream PostSync3(HttpClient client, string url, HttpContent content)
        {
            return Task.Run(() =>
                       Task.Run(() =>
                           client.PostAsync(url, content))
                       .Result.Content.ReadAsStreamAsync())
                   .Result;
        }
    }
}
