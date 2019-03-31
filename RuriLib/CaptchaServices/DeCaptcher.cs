using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.CaptchaServices
{
    /// <summary>
    /// Implements the API of http://de-captcher.com/
    /// </summary>
    public class DeCaptcher : CaptchaService
    {
        /// <summary>
        /// Creates a DeCaptcher client.
        /// </summary>
        /// <param name="user">The DeCaptcher username</param>
        /// <param name="pass">The DeCaptcher password</param>
        /// <param name="timeout">The maximum time to wait for the challenge to be solved</param>
        public DeCaptcher(string user, string pass, int timeout) : base(user, pass, timeout) { }

        /// <inheritdoc />
        public override double GetBalance()
        {
            using (HttpClient client = new HttpClient())
            using (MultipartFormDataContent data = new MultipartFormDataContent())
            {
                client.Timeout = new TimeSpan(0, 0, Timeout);
                data.Add(new StringContent(User), "username");
                data.Add(new StringContent(Pass), "password");
                data.Add(new StringContent("balance"), "function");
                var response = PostSync(client, "http://poster.de-captcher.com/", data);
                return double.Parse(response, CultureInfo.InvariantCulture);
            }
        }

        /// <inheritdoc />
        public override string SolveRecaptcha(string siteKey, string siteUrl)
        {
            using (HttpClient client = new HttpClient())
            using (MultipartFormDataContent data = new MultipartFormDataContent())
            {
                client.Timeout = new TimeSpan(0, 0, Timeout);
                data.Add(new StringContent(User), "username");
                data.Add(new StringContent(Pass), "password");
                data.Add(new StringContent("proxyurl"), "function");
                data.Add(new StringContent(siteKey), "key");
                data.Add(new StringContent(siteUrl), "url");
                var response = PostSync(client, "http://poster.de-captcher.com/", data);
                try { return response.Split('|')[5]; }
                catch { throw new Exception(response); }
            }
        }

        /// <inheritdoc />
        public override string SolveCaptcha(Bitmap bitmap)
        {
            using(HttpClient client = new HttpClient())
            using (MultipartFormDataContent data = new MultipartFormDataContent())
            {
                client.Timeout = new TimeSpan(0, 0, Timeout);
                data.Add(new StringContent(User), "username");
                data.Add(new StringContent(Pass), "password");
                data.Add(new StringContent("picture2"), "function");
                data.Add(new ByteArrayContent(GetBytes(bitmap)), "pict");
                data.Add(new StringContent("0"), "picttype");
                var response = PostSync(client, "http://poster.de-captcher.com/", data);
                try { return response.Split('|')[5]; }
                catch { throw new Exception(response); }
            }
        }
    }
}
