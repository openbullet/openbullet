using RuriLib.LS;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Threading;
using Jint;
using System.Net;
using Cloudflare;
using Cloudflare.CaptchaProviders;
using System.Net.Http;

namespace RuriLib
{
    /// <summary>
    /// A block that can bypass Cloudflare protections.
    /// </summary>
    public class BlockBypassCF : BlockBase
    {
        private string url = "";
        /// <summary>The URL of the Cloudflare-protected website.</summary>
        public string Url { get { return url; } set { url = value; OnPropertyChanged(); } }

        private string userAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko";
        /// <summary>The User-Agent header to use when solving the challenge.</summary>
        public string UserAgent { get { return userAgent; } set { userAgent = value; OnPropertyChanged(); } }

        /// <summary>
        /// Creates a Cloudflare bypass block.
        /// </summary>
        public BlockBypassCF()
        {
            Label = "BYPASS CF";
        }

        /// <inheritdoc />
        public override BlockBase FromLS(string line)
        {
            // Trim the line
            var input = line.Trim();

            // Parse the label
            if (input.StartsWith("#"))
                Label = LineParser.ParseLabel(ref input);

            /*
             * Syntax
             * BYPASSCF "URL" ["UA"]
             * */

            Url = LineParser.ParseLiteral(ref input, "URL");

            if (input != "") UserAgent = LineParser.ParseLiteral(ref input, "UA");

            return this;
        }

        /// <inheritdoc />
        public override string ToLS(bool indent = true)
        {
            var writer = new BlockWriter(GetType(), indent, Disabled);
            writer
                .Label(Label)
                .Token("BYPASSCF")
                .Literal(Url)
                .Literal(UserAgent, "UserAgent");
            return writer.ToString();
        }

        /// <inheritdoc />
        public override void Process(BotData data)
        {
            base.Process(data);

            // If the clearance info is already set and we're not getting it fresh each time, skip
            if (data.UseProxies)
            {
                if(data.Proxy.Clearance != "" && data.Proxy.Cfduid != "" && !data.GlobalSettings.Proxies.AlwaysGetClearance)
                {
                    data.Log(new LogEntry("Skipping CF Bypass because there is already a valid cookie", Colors.White));
                    return;
                }
            }

            var localUrl = ReplaceValues(url, data);

            var timeout = data.GlobalSettings.General.RequestTimeout * 1000;

            // We initialize the solver basing on the captcha service available
            CloudflareSolver cf = null;
            switch (data.GlobalSettings.Captchas.CurrentService)
            {
                case BlockCaptcha.CaptchaService.AntiCaptcha:
                    cf = new CloudflareSolver(new AntiCaptchaProvider(data.GlobalSettings.Captchas.AntiCapToken));
                    break;

                case BlockCaptcha.CaptchaService.TwoCaptcha:
                    cf = new CloudflareSolver(new TwoCaptchaProvider(data.GlobalSettings.Captchas.TwoCapToken));
                    break;

                default:
                    cf = new CloudflareSolver();
                    break;
            }

            // Initialize the handler with the Proxy and the previous cookies
            HttpClientHandler handler = null;

            CookieContainer cookies = new CookieContainer();
            foreach (var cookie in data.Cookies)
                cookies.Add(new Cookie(cookie.Key, cookie.Value));

            if (data.UseProxies)
            {
                if (data.Proxy.Type != Extreme.Net.ProxyType.Http)
                {
                    throw new Exception($"The proxy type {data.Proxy.Type} is not supported by this block yet");
                }

                var proxy = new WebProxy(data.Proxy.Proxy, false);

                if (data.Proxy.Username != "")
                {
                    proxy.Credentials = new NetworkCredential(data.Proxy.Username, data.Proxy.Password);
                }

                handler = new HttpClientHandler()
                {
                    Proxy = proxy,
                    CookieContainer = cookies,
                    AllowAutoRedirect = true,
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                };
            }
            else
            {
                handler = new HttpClientHandler()
                {
                    CookieContainer = cookies,
                    AllowAutoRedirect = true,
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                };
            }
            
            // Initialize the HttpClient with the given handler, timeout, user-agent
            var httpClient = new HttpClient(handler);
            httpClient.Timeout = TimeSpan.FromMinutes(timeout);
            httpClient.DefaultRequestHeaders.Add("User-Agent", ReplaceValues(userAgent, data));

            var uri = new Uri(localUrl);

            // Solve the CF challenge
            var result = cf.Solve(httpClient, handler, uri).Result;
            if (result.Success)
            {
                data.Log(new LogEntry($"[Success] Protection bypassed: {result.DetectResult.Protection}", Colors.GreenYellow));
            }
            else
            {
                throw new Exception($"CF Bypass Failed: {result.FailReason}");
            }

            // Once the protection has been bypassed we can use that httpClient to send the requests as usual
            var response = httpClient.GetAsync(uri).Result;

            // Save the cookies
            var ck = cookies.GetCookies(uri);

            var clearance = "";
            var cfduid = "";

            try
            {
                clearance = ck["cf_clearance"].Value;
                cfduid = ck["__cfduid"].Value;
            }
            catch { }

            if (data.UseProxies)
            {
                data.Proxy.Clearance = clearance;
                data.Proxy.Cfduid = cfduid;
            }

            data.Log(new LogEntry("Got Cloudflare clearance!", Colors.GreenYellow));
            data.Log(new LogEntry(clearance + Environment.NewLine + cfduid + Environment.NewLine, Colors.White));

            // Get code
            data.ResponseCode = ((int)response.StatusCode).ToString();
            data.Log(new LogEntry("Response code: " + data.ResponseCode, Colors.Cyan));

            // Get headers
            data.Log(new LogEntry("Received headers:", Colors.DeepPink));
            var headerList = new List<KeyValuePair<string, string>>();
            var receivedHeaders = response.Headers.GetEnumerator();
            data.ResponseHeaders.Clear();

            while (receivedHeaders.MoveNext())
            {
                var header = receivedHeaders.Current;
                var value = header.Value.GetEnumerator();
                if (value.MoveNext())
                {
                    data.ResponseHeaders.Add(header.Key, value.Current);
                    data.Log(new LogEntry(header.Key + ": " + value.Current, Colors.LightPink));
                }
            }

            // Get cookies
            data.Log(new LogEntry("Received cookies:", Colors.Goldenrod));
            foreach (Cookie cookie in ck)
            {
                if (data.Cookies.ContainsKey(cookie.Name)) data.Cookies[cookie.Name] = cookie.Value;
                else data.Cookies.Add(cookie.Name, cookie.Value);
                data.Log(new LogEntry(cookie.Name + ": " + cookie.Value, Colors.LightGoldenrodYellow));
            }

            // Save the content
            data.ResponseSource = response.Content.ReadAsStringAsync().Result;
            data.Log(new LogEntry("Response Source:", Colors.Green));
            data.Log(new LogEntry(data.ResponseSource, Colors.GreenYellow));
        }
    }
}
