using Extreme.Net;
using RuriLib.CaptchaServices;
using RuriLib.LS;
using RuriLib.Models;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Media;

namespace RuriLib
{
    /// <summary>
    /// A block that solves an image captcha challenge.
    /// </summary>
    public class BlockImageCaptcha : BlockCaptcha
    {
        private string url = "";
        /// <summary>The URL to download the captcha image from.</summary>
        public string Url { get { return url; } set { url = value; OnPropertyChanged(); } }

        private string variableName = "";
        /// <summary>The name of the variable where the challenge solution will be stored.</summary>
        public string VariableName { get { return variableName; } set { variableName = value; OnPropertyChanged(); } }

        private bool base64 = false;
        /// <summary>Whether the Url is a base64-encoded captcha image.</summary>
        public bool Base64 { get { return base64; } set { base64 = value; OnPropertyChanged(); } }

        private bool sendScreenshot = false;
        /// <summary>Whether the captcha image needs to be taken by the last screenshot taken by selenium.</summary>
        public bool SendScreenshot { get { return sendScreenshot; } set { sendScreenshot = value; OnPropertyChanged(); } }

        /// <summary>
        /// Creates an Image Captcha block.
        /// </summary>
        public BlockImageCaptcha()
        {
            Label = "CAPTCHA";
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
             * Syntax:
             * CAPTCHA "URL" [BASE64? USESCREEN?] -> VAR "CAP"
             * */

            Url = LineParser.ParseLiteral(ref input, "URL");

            while (LineParser.Lookahead(ref input) == TokenType.Boolean)
                LineParser.SetBool(ref input, this);

            LineParser.EnsureIdentifier(ref input, "->");
            LineParser.EnsureIdentifier(ref input, "VAR");

            // Parse the variable name
            VariableName = LineParser.ParseLiteral(ref input, "VARIABLE NAME");

            return this;
        }

        /// <inheritdoc />
        public override string ToLS(bool indent = true)
        {
            var writer = new BlockWriter(GetType(), indent, Disabled);
            writer
                .Label(Label)
                .Token("CAPTCHA")
                .Literal(Url)
                .Boolean(Base64, "Base64")
                .Boolean(SendScreenshot, "SendScreenshot")
                .Arrow()
                .Token("VAR")
                .Literal(VariableName);
            return writer.ToString();
        }

        /// <inheritdoc />
        public override void Process(BotData data)
        {
            if(!data.GlobalSettings.Captchas.BypassBalanceCheck)
                base.Process(data);

            var localUrl = ReplaceValues(url, data);

            data.Log(new LogEntry("Downloading image...", Colors.White));

            // Download captcha
            var captchaFile = string.Format("Captchas/captcha{0}.jpg", data.BotNumber);
            if (base64)
            {
                var bytes = Convert.FromBase64String(localUrl);
                using (var imageFile = new FileStream(captchaFile, FileMode.Create))
                {
                    imageFile.Write(bytes, 0, bytes.Length);
                    imageFile.Flush();
                }
            }
            else if (sendScreenshot && data.Screenshots.Count > 0)
            {
                Bitmap image = new Bitmap(data.Screenshots.Last());
                image.Save(captchaFile);
            }
            else
            {
                try
                {
                    DownloadRemoteImageFile(captchaFile, data, localUrl);
                }
                catch (Exception ex) { data.Log(new LogEntry(ex.Message, Colors.Tomato)); throw; }
            }

            string response = "";
            CaptchaServices.CaptchaService service = null;
            var bitmap = new Bitmap(captchaFile);
            try
            {
                switch (data.GlobalSettings.Captchas.CurrentService)
                {
                    case CaptchaService.ImageTypers:
                        service = new ImageTyperz(data.GlobalSettings.Captchas.ImageTypToken, data.GlobalSettings.Captchas.Timeout);
                        break;

                    case CaptchaService.AntiCaptcha:
                        service = new AntiCaptcha(data.GlobalSettings.Captchas.AntiCapToken, data.GlobalSettings.Captchas.Timeout);
                        break;

                    case CaptchaService.DBC:
                        service = new DeathByCaptcha(data.GlobalSettings.Captchas.DBCUser, data.GlobalSettings.Captchas.DBCPass, data.GlobalSettings.Captchas.Timeout);
                        break;

                    case CaptchaService.TwoCaptcha:
                        service = new TwoCaptcha(data.GlobalSettings.Captchas.TwoCapToken, data.GlobalSettings.Captchas.Timeout);
                        break;

                    case CaptchaService.DeCaptcher:
                        service = new DeCaptcher(data.GlobalSettings.Captchas.DCUser, data.GlobalSettings.Captchas.DCPass, data.GlobalSettings.Captchas.Timeout);
                        break;

                    case CaptchaService.AZCaptcha:
                        service = new AZCaptcha(data.GlobalSettings.Captchas.AZCapToken, data.GlobalSettings.Captchas.Timeout);
                        break;

                    case CaptchaService.CaptchasIO:
                        service = new CaptchasIO(data.GlobalSettings.Captchas.CIOToken, data.GlobalSettings.Captchas.Timeout);
                        break;

                    default:
                        throw new Exception("This service cannot solve normal captchas!");
                }
                response = service.SolveCaptcha(bitmap);
            }
            catch(Exception ex) { data.Log(new LogEntry(ex.Message, Colors.Tomato)); throw; }
            finally { bitmap.Dispose(); }

            data.CaptchaService = service;
            data.Log(response == "" ? new LogEntry("Couldn't get a response from the service", Colors.Tomato) : new LogEntry("Succesfully got the response: " + response, Colors.GreenYellow));

            if (VariableName != "")
            {
                data.Log(new LogEntry("Response stored in variable: " + variableName, Colors.White));
                data.Variables.Set(new CVar(variableName, response));
            }
        }

        private void DownloadRemoteImageFile(string fileName, BotData data, string localUrl)
        {
            HttpRequest request = new HttpRequest();

            request.Cookies = new CookieDictionary();
            foreach (var cookie in data.Cookies)
                request.Cookies.Add(cookie.Key, cookie.Value);

            // Set proxy
            if (data.UseProxies)
            {
                request.Proxy = data.Proxy.GetClient();

                var timeout = data.GlobalSettings.General.RequestTimeout * 1000;
                try
                {
                    request.Proxy.ReadWriteTimeout = timeout;
                    request.Proxy.ConnectTimeout = timeout;
                    request.Proxy.Username = data.Proxy.Username;
                    request.Proxy.Password = data.Proxy.Password;
                }
                catch { }
            }

            HttpResponse response = request.Get(localUrl);
            
            using (Stream inputStream = response.ToMemoryStream())
            using (Stream outputStream = File.OpenWrite(fileName))
            {
                byte[] buffer = new byte[4096];
                int bytesRead;
                do
                {
                    bytesRead = inputStream.Read(buffer, 0, buffer.Length);
                    outputStream.Write(buffer, 0, bytesRead);
                } while (bytesRead != 0);
            }

            data.Cookies = response.Cookies;
        }
    }
}
