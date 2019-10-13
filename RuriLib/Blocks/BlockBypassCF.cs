using RuriLib.LS;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Threading;
using Jint;
using Leaf.xNet;
using System.Net;
using Leaf.xNet.Services.Cloudflare;
using Leaf.xNet.Services.Captcha;

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
                    data.Cookies.Add("cf_clearance", data.Proxy.Clearance);
                    return;
                }
            }

            var localUrl = ReplaceValues(url, data);
            var uri = new Uri(localUrl);
            
            var timeout = data.GlobalSettings.General.RequestTimeout * 1000;

            var request = new HttpRequest();
            request.IgnoreProtocolErrors = true;
            request.ConnectTimeout = timeout;
            request.ReadWriteTimeout = timeout;
            request.Cookies = new CookieStorage();
            foreach (var cookie in data.Cookies)
                request.Cookies.Add(new Cookie(cookie.Key, cookie.Value, "/", uri.Host));

            if (data.UseProxies)
            {
                switch (data.Proxy.Type)
                {
                    case Extreme.Net.ProxyType.Http:
                        request.Proxy = HttpProxyClient.Parse(data.Proxy.Proxy);
                        break;

                    case Extreme.Net.ProxyType.Socks4:
                        request.Proxy = Socks4ProxyClient.Parse(data.Proxy.Proxy);
                        break;

                    case Extreme.Net.ProxyType.Socks4a:
                        request.Proxy = Socks4AProxyClient.Parse(data.Proxy.Proxy);
                        break;

                    case Extreme.Net.ProxyType.Socks5:
                        request.Proxy = Socks5ProxyClient.Parse(data.Proxy.Proxy);
                        break;

                    case Extreme.Net.ProxyType.Chain:
                        throw new Exception("The Chain Proxy Type is not supported in Leaf.xNet (used for CF Bypass).");
                }

                request.Proxy.ReadWriteTimeout = timeout;
                request.Proxy.ConnectTimeout = timeout;
                request.Proxy.Username = data.Proxy.Username;
                request.Proxy.Password = data.Proxy.Password;
            }

            request.UserAgent = ReplaceValues(userAgent, data);

            var twoCapToken = data.GlobalSettings.Captchas.TwoCapToken;
            if (twoCapToken != "") request.CaptchaSolver = new TwoCaptchaSolver() { ApiKey = data.GlobalSettings.Captchas.TwoCapToken };

            var response = request.GetThroughCloudflare(new Uri(localUrl));
            var responseString = response.ToString();

            // Save the cookies
            var ck = response.Cookies.GetCookies(localUrl);

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
            var receivedHeaders = response.EnumerateHeaders();
            data.ResponseHeaders.Clear();
            while (receivedHeaders.MoveNext())
            {
                var header = receivedHeaders.Current;
                data.ResponseHeaders.Add(header.Key, header.Value);
                if (PrintResponseInfo) data.Log(new LogEntry($"{header.Key}: {header.Value}", Colors.LightPink));
            }
            if (!response.ContainsHeader(HttpHeader.ContentLength))
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
            foreach (Cookie cookie in response.Cookies.GetCookies(localUrl))
            {
                if (data.Cookies.ContainsKey(cookie.Name)) data.Cookies[cookie.Name] = cookie.Value;
                else data.Cookies.Add(cookie.Name, cookie.Value);
                if (PrintResponseInfo) data.Log(new LogEntry($"{cookie.Name}: {cookie.Value}", Colors.LightGoldenrodYellow));
            }

            data.ResponseSource = responseString;
            if (PrintResponseInfo)
            {
                data.Log(new LogEntry("Response Source:", Colors.Green));
                data.Log(new LogEntry(data.ResponseSource, Colors.GreenYellow));
            }

            if (ErrorOn302 && data.ResponseCode.Contains("302"))
            {
                data.Status = BotStatus.ERROR;
            };
        }
    }
}
