using RuriLib.LS;
using System;
using System.Windows.Media;
using Jint;
using System.Net;
using CloudflareSolverRe;
using CloudflareSolverRe.Types;
using CloudflareSolverRe.CaptchaProviders;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;

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

        private bool printResponseInfo = true;
        /// <summary>Whether to print the full response info to the log.</summary>
        public bool PrintResponseInfo { get { return printResponseInfo; } set { printResponseInfo = value; OnPropertyChanged(); } }

        private bool errorOn302 = true;
        /// <summary>Whether to set ERROR Status on 302.</summary>
        public bool ErrorOn302 { get { return errorOn302; } set { errorOn302 = value; OnPropertyChanged(); } }

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

            if (input != "" && LineParser.Lookahead(ref input) == TokenType.Literal)
            {
                UserAgent = LineParser.ParseLiteral(ref input, "UA");
            }

            while (input != "")
            {
                LineParser.SetBool(ref input, this);
            }

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
                .Literal(UserAgent, "UserAgent")
                .Boolean(PrintResponseInfo, "PrintResponseInfo")
                .Boolean(ErrorOn302, "ErrorOn302");
            return writer.ToString();
        }

        /// <inheritdoc />
        public override void Process(BotData data)
        {
            base.Process(data);

            // If the clearance info is already set and we're not getting it fresh each time, skip
            if (data.UseProxies)
            {
                if (data.Proxy.Clearance != "" && !data.GlobalSettings.Proxies.AlwaysGetClearance)
                {
                    data.Log(new LogEntry("Skipping CF Bypass because there is already a valid cookie", Colors.White));
                    data.Cookies["cf_clearance"] = data.Proxy.Clearance;
                    return;
                }
            }

            var localUrl = ReplaceValues(url, data);
            var uri = new Uri(localUrl);
            
            var timeout = data.GlobalSettings.General.RequestTimeout * 1000;

            // Initialize the captcha provider
            // TODO: Add more providers by implementing the ICaptchaProvider interface on the missing ones
            ICaptchaProvider provider = null;
            switch (data.GlobalSettings.Captchas.CurrentService)
            {
                case CaptchaServices.ServiceType.AntiCaptcha:
                    provider = new AntiCaptchaProvider(data.GlobalSettings.Captchas.AntiCapToken);
                    break;

                case CaptchaServices.ServiceType.TwoCaptcha:
                    provider = new TwoCaptchaProvider(data.GlobalSettings.Captchas.TwoCapToken);
                    break;
            }

            // Initialize the Cloudflare Solver
            CloudflareSolver cf = new CloudflareSolver(provider, ReplaceValues(UserAgent, data));
            cf.ClearanceDelay = 3000;
            cf.MaxCaptchaTries = 1;
            cf.MaxTries = 3;

            // Create the cookie container
            CookieContainer cookies = new CookieContainer();
            foreach (var cookie in data.Cookies)
            {
                cookies.Add(new Cookie(cookie.Key, cookie.Value, "/", uri.Host));
            }

            // Initialize the http handler
            HttpClientHandler handler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                CookieContainer = cookies,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            // Assign the proxy to the inner handler if necessary
            if (data.UseProxies)
            {
                if (data.Proxy.Type != Extreme.Net.ProxyType.Http)
                {
                    throw new Exception($"The proxy type {data.Proxy.Type} is not supported by this block yet");
                }

                handler.Proxy = new WebProxy(data.Proxy.Proxy, false);
                handler.UseProxy = true;

                if (!string.IsNullOrEmpty(data.Proxy.Username))
                {
                    handler.DefaultProxyCredentials = new NetworkCredential(data.Proxy.Username, data.Proxy.Password);
                }
            }

            // Initialize the http client
            HttpClient http = new HttpClient(handler);
            http.Timeout = TimeSpan.FromMinutes(timeout);
            http.DefaultRequestHeaders.Add("User-Agent", ReplaceValues(UserAgent, data));

            var result = cf.Solve(http, handler, uri, ReplaceValues(UserAgent, data)).Result;

            if (result.Success)
            {
                data.Log(new LogEntry($"[Success] Protection bypassed: {result.DetectResult.Protection}", Colors.GreenYellow));
            }
            else if (result.DetectResult.Protection == CloudflareProtection.Unknown)
            {
                data.Log(new LogEntry($"Unknown protection, skipping the bypass!", Colors.Tomato));
            }
            else
            {
                throw new Exception($"CF Bypass Failed: {result.FailReason}");
            }

            // Now that we got the cookies, proceed with the normal request
            HttpResponseMessage response = null;
            try
            {
                response = http.GetAsync(uri).Result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                handler.Dispose();
                http.Dispose();
            }

            var responseString = response.Content.ReadAsStringAsync().Result;

            // Save the cloudflare cookies
            var clearance = "";
            var cfduid = "";
            foreach (Cookie cookie in cookies.GetCookies(uri))
            {
                switch (cookie.Name)
                {
                    case "cf_clearance":
                        clearance = cookie.Value;
                        break;

                    case "__cfduid":
                        cfduid = cookie.Value;
                        break;
                }
            }

            // Save the cookies in the proxy
            if (data.UseProxies)
            {
                data.Proxy.Clearance = clearance;
                data.Proxy.Cfduid = cfduid;
            }

            if (clearance != "")
            {
                data.Log(new LogEntry("Got Cloudflare clearance!", Colors.GreenYellow));
                data.Log(new LogEntry(clearance + Environment.NewLine + cfduid + Environment.NewLine, Colors.White));
            }

            // Get code
            data.ResponseCode = ((int)response.StatusCode).ToString();
            if (PrintResponseInfo) data.Log(new LogEntry($"Response code: {data.ResponseCode}", Colors.Cyan));

            // Get headers
            if (PrintResponseInfo) data.Log(new LogEntry("Received headers:", Colors.DeepPink));
            data.ResponseHeaders.Clear();
            foreach (var header in response.Headers)
            {
                var h = new KeyValuePair<string, string>(header.Key, header.Value.First());
                data.ResponseHeaders.Add(h.Key, h.Value);
                if (PrintResponseInfo) data.Log(new LogEntry($"{h.Key}: {h.Value}", Colors.LightPink));
            }

            // Add the Content-Length header if it was not sent by the server
            if (!data.ResponseHeaders.ContainsKey("Content-Length"))
            {
                if (data.ResponseHeaders.ContainsKey("Content-Encoding") && data.ResponseHeaders["Content-Encoding"].Contains("gzip"))
                {
                    data.ResponseHeaders["Content-Length"] = GZip.Zip(responseString).Length.ToString();
                }
                else
                {
                    data.ResponseHeaders["Content-Length"] = responseString.Length.ToString();
                }

                if (PrintResponseInfo) data.Log(new LogEntry($"Content-Length: {data.ResponseHeaders["Content-Length"]}", Colors.LightPink));
            }

            // Get cookies
            if (PrintResponseInfo) data.Log(new LogEntry("Received cookies:", Colors.Goldenrod));
            foreach (Cookie cookie in cookies.GetCookies(uri))
            {
                data.Cookies[cookie.Name] = cookie.Value;
                if (PrintResponseInfo) data.Log(new LogEntry($"{cookie.Name}: {cookie.Value}", Colors.LightGoldenrodYellow));
            }

            // Print source
            data.ResponseSource = responseString;
            if (PrintResponseInfo)
            {
                data.Log(new LogEntry("Response Source:", Colors.Green));
                data.Log(new LogEntry(data.ResponseSource, Colors.GreenYellow));
            }

            // Error on 302 status
            if (ErrorOn302 && response.StatusCode == HttpStatusCode.Redirect)
            {
                data.Status = BotStatus.ERROR;
            }
        }
    }
}
