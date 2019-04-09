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

            var request = new HttpRequest();
            request.IgnoreProtocolErrors = true;            
            request.ConnectTimeout = timeout;
            request.ReadWriteTimeout = timeout;
            request.Cookies = new CookieStorage();
            foreach (var cookie in data.Cookies)
                request.Cookies.Add(new Cookie(cookie.Key, cookie.Value));

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
            var cookies = response.Cookies.GetCookies(localUrl);
            var clearanceCookie = cookies["cf_clearance"];
            var cfduidCookie = cookies["__cfduid"];
            data.Cookies.Add("cf_clearance", clearanceCookie.Value);
            data.Cookies.Add("__cfduid", cfduidCookie.Value);
            if (data.UseProxies)
            {
                data.Proxy.Clearance = clearanceCookie.Value;
                data.Proxy.Cfduid = cfduidCookie.Value;
            }
            data.Log(new LogEntry("Got Cloudflare clearance!", Colors.GreenYellow));
            data.Log(new LogEntry(clearanceCookie + Environment.NewLine + cfduidCookie + Environment.NewLine, Colors.White));

            // Get code
            data.ResponseCode = ((int)response.StatusCode).ToString();
            data.Log(new LogEntry("Response code: " + data.ResponseCode, Colors.Cyan));

            // Get headers
            data.Log(new LogEntry("Received headers:", Colors.DeepPink));
            var headerList = new List<KeyValuePair<string, string>>();
            var receivedHeaders = response.EnumerateHeaders();
            data.ResponseHeaders.Clear();
            while (receivedHeaders.MoveNext())
            {
                var header = receivedHeaders.Current;
                data.ResponseHeaders.Add(header.Key, header.Value);
                data.Log(new LogEntry(header.Key + ": " + header.Value, Colors.LightPink));
            }

            // Get cookies
            data.Log(new LogEntry("Received cookies:", Colors.Goldenrod));
            foreach (Cookie cookie in response.Cookies.GetCookies(localUrl))
            {
                if (data.Cookies.ContainsKey(cookie.Name)) data.Cookies[cookie.Name] = cookie.Value;
                else data.Cookies.Add(cookie.Name, cookie.Value);
                data.Log(new LogEntry(cookie.Name + ": " + cookie.Value, Colors.LightGoldenrodYellow));
            }

            data.ResponseSource = response.ToString();
            data.Log(new LogEntry("Response Source:", Colors.Green));
            data.Log(new LogEntry(data.ResponseSource, Colors.GreenYellow));
        }
    }
}
