using Extreme.Net;
using RuriLib.Functions.Formats;
using RuriLib.Models;
using RuriLib.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RuriLib.Functions.Files;
using System.Windows.Media;
using RuriLib.Functions.Conversions;

namespace RuriLib.Functions.Requests
{
    /// <summary>
    /// Enumerates the supported security protocols.
    /// </summary>
    public enum SecurityProtocol
    {
        /// <summary>Let the operative system decide and block the unsecure protocols.</summary>
        SystemDefault,

        /// <summary>The SSL3 protocol (obsolete).</summary>
        SSL3,

        /// <summary>The TLS 1.0 protocol (obsolete).</summary>
        TLS10,

        /// <summary>The TLS 1.1 protocol.</summary>
        TLS11,

        /// <summary>The TLS 1.2 protocol.</summary>
        TLS12
    }

    /// <summary>
    /// Provides methods to easily perform Extreme.NET requests.
    /// </summary>
    public class Request
    {
        private HttpRequest request = new HttpRequest();
        private HttpContent content = null;
        private Dictionary<string, string> oldCookies = new Dictionary<string, string>();
        private int timeout = 60000;
        private string contentType = "";
        private string authorization = "";

        private HttpResponse response = null;
        private bool hasContentLength = true;
        private bool isGZipped = false;

        /// <summary>
        /// Disposes of the HttpRequest and HttpContent when destroyed.
        /// </summary>
        ~Request()
        {
            request?.Dispose();
            content?.Dispose();
        }

        /// <summary>
        /// Sets up the request options.
        /// </summary>
        /// <param name="settings">The RuriLib settings</param>
        /// <param name="securityProtocol">The security protocol to use</param>
        /// <param name="autoRedirect">Whether to perform automatic redirection</param>
        /// <param name="acceptEncoding"></param>
        /// <param name="maxRedirects"></param>
        /// <returns></returns>
        public Request Setup(RLSettingsViewModel settings, SecurityProtocol securityProtocol = SecurityProtocol.SystemDefault,
            bool autoRedirect = true, int maxRedirects = 8, bool acceptEncoding = true)
        {
            // Setup options
            timeout = settings.General.RequestTimeout * 1000;
            request.IgnoreProtocolErrors = true;
            request.AllowAutoRedirect = autoRedirect;
            request.EnableEncodingContent = acceptEncoding;
            request.ReadWriteTimeout = timeout;
            request.ConnectTimeout = timeout;
            request.KeepAlive = true;
            request.MaximumAutomaticRedirections = maxRedirects;
            request.SslProtocols = securityProtocol.ToSslProtocols();

            return this;
        }

        /// <summary>
        /// Sets a standard content for the request.
        /// </summary>
        /// <param name="postData">The content string</param>
        /// <param name="contentType">The content type</param>
        /// <param name="method">The HTTP method</param>
        /// <param name="encodeContent">Whether to URLencode the content automatically</param>
        /// <param name="log">The log (if any)</param>
        /// <returns>The request itself</returns>
        public Request SetStandardContent(string postData, string contentType,
            HttpMethod method = HttpMethod.POST, bool encodeContent = false, List<LogEntry> log = null)
        {
            this.contentType = contentType;
            var pData = Regex.Replace(postData, @"(?<!\\)\\n", Environment.NewLine).Unescape();

            if (HttpRequest.CanContainRequestBody(method))
            {
                if (encodeContent)
                {
                    // Very dirty but it works
                    Random rand = new Random();
                    var nonce = rand.Next(1000000, 9999999);
                    pData = pData.Replace("&", $"{nonce}&{nonce}").Replace("=", $"{nonce}={nonce}");
                    pData = string.Join("", BlockFunction.SplitInChunks(pData, 2080)
                        .Select(s => Uri.EscapeDataString(s)))
                        .Replace($"{nonce}%26{nonce}", "&").Replace($"{nonce}%3D{nonce}", "=");
                }

                content = new StringContent(pData);
                content.ContentType = contentType;

                if (log != null) log.Add(new LogEntry($"Post Data: {pData}", Colors.MediumTurquoise));
            }

            return this;
        }

        /// <summary>
        /// Sets a raw content for the request.
        /// </summary>
        /// <param name="rawData">The raw HEX data</param>
        /// <param name="contentType">The content type</param>
        /// <param name="method">The HTTP method</param>
        /// <param name="log">The log (if any)</param>
        /// <returns>The request itself</returns>
        public Request SetRawContent(string rawData, string contentType,
            HttpMethod method = HttpMethod.POST, List<LogEntry> log = null)
        {
            this.contentType = contentType;
            var rData = Conversion.ConvertFrom(rawData, Conversions.Encoding.HEX);

            if (HttpRequest.CanContainRequestBody(method))
            {
                var ms = new MemoryStream(rData);
                content = new StreamContent(ms);
                content.ContentType = contentType;

                if (log != null) log.Add(new LogEntry($"Raw Data: {rawData}", Colors.MediumTurquoise));
            }

            return this;
        }

        /// <summary>
        /// Sets the authorization header for basic authorization.
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="password">The password</param>
        /// <returns>The request itself</returns>
        public Request SetBasicAuth(string username, string password)
        {
            authorization = "Basic " + (username + ":" + password).ToBase64();

            return this;
        }

        /// <summary>
        /// Sets a multipart content collection for the request.
        /// </summary>
        /// <param name="contents">A collection of multipart contents</param>
        /// <param name="boundary">A boundary (if empty, one will be generated automatically)</param>
        /// <param name="log">The log (if any)</param>
        /// <returns>The request itself</returns>
        public Request SetMultipartContent(IEnumerable<MultipartContent> contents, string boundary = "", List<LogEntry> log = null)
        {
            var bdry = boundary != string.Empty ? boundary : GenerateMultipartBoundary();
            content = new Extreme.Net.MultipartContent(bdry);
            var mContent = content as Extreme.Net.MultipartContent;
            
            if (log != null)
            {
                log.Add(new LogEntry($"Content-Type: multipart/form-data; boundary={bdry}", Colors.MediumTurquoise));
                log.Add(new LogEntry("Multipart Data:", Colors.MediumTurquoise));
                log.Add(new LogEntry(bdry, Colors.MediumTurquoise));
            }

            foreach (var c in contents)
            {
                if (c.Type == MultipartContentType.String)
                {
                    mContent.Add(new StringContent(c.Value.Unescape()), c.Name);
                    if (log != null) log.Add(new LogEntry($"Content-Disposition: form-data; name=\"{c.Name}\"{Environment.NewLine}{Environment.NewLine}{c.Value.Unescape()}", Colors.MediumTurquoise));
                }
                else if (c.Type == MultipartContentType.File)
                {
                    mContent.Add(new FileContent(c.Value), c.Name, c.Value, c.ContentType);
                    if (log != null) log.Add(new LogEntry($"Content-Disposition: form-data; name=\"{c.Name}\"; filename=\"{c.Value}\"{Environment.NewLine}Content-Type: {c.ContentType}{Environment.NewLine}{Environment.NewLine}[FILE CONTENT OMITTED]", Colors.MediumTurquoise));
                }
                
                if (log != null) log.Add(new LogEntry(bdry, Colors.MediumTurquoise));
            }

            return this;
        }

        /// <summary>
        /// Sets a proxy to be used during the request.
        /// </summary>
        /// <param name="proxy">The proxy</param>
        /// <returns>The request itself</returns>
        public Request SetProxy(CProxy proxy)
        {
            request.Proxy = proxy.GetClient();

            request.Proxy.ReadWriteTimeout = timeout;
            request.Proxy.ConnectTimeout = timeout;
            request.Proxy.Username = proxy.Username;
            request.Proxy.Password = proxy.Password;

            return this;
        }

        /// <summary>
        /// Sets the cookies to be sent in the request.
        /// </summary>
        /// <param name="cookies">The cookie dictionary</param>
        /// <param name="log">The log (if any)</param>
        /// <returns>The request itself</returns>
        public Request SetCookies(Dictionary<string, string> cookies, List<LogEntry> log = null)
        {
            oldCookies = cookies;
            request.Cookies = new CookieDictionary();

            foreach (var cookie in cookies)
            {
                request.Cookies.Add(cookie.Key, cookie.Value);
                if (log != null) log.Add(new LogEntry($"{cookie.Key}: {cookie.Value}", Colors.MediumTurquoise));
            }

            return this;
        }

        /// <summary>
        /// Sets the headers to be sent in the request.
        /// </summary>
        /// <param name="headers">The headers dictionary</param>
        /// <param name="acceptEncoding">Whether to set the Accept-Encoding header automatically</param>
        /// <param name="log">The log (if any)</param>
        /// <returns>The request itself</returns>
        public Request SetHeaders(Dictionary<string, string> headers, bool acceptEncoding = true, List<LogEntry> log = null)
        {
            // Set headers
            foreach (var header in headers)
            {
                try
                {
                    var replacedKey = header.Key.Replace("-", "").ToLower(); // Used to compare with the HttpHeader enum

                    if (replacedKey == "contenttype" && content != null) { continue; } // Disregard additional Content-Type headers
                    if (replacedKey == "acceptencoding" && acceptEncoding) { continue; } // Disregard additional Accept-Encoding headers
                    // else if (fixedNames.Contains(replacedKey)) request.AddHeader((HttpHeader)Enum.Parse(typeof(HttpHeader), replacedKey, true), val);
                    else request.AddHeader(header.Key, header.Value);

                    if (log != null) log.Add(new LogEntry($"{header.Key}: {header.Value}", Colors.MediumTurquoise));
                }
                catch { }
            }

            // Add the authorization header on a Basic Auth request
            if (authorization != string.Empty)
            {
                request.AddHeader("Authorization", authorization);
                if (log != null) log.Add(new LogEntry($"Authorization: {authorization}", Colors.MediumTurquoise));
            }

            // Add the content-type header
            if (contentType != string.Empty)
            {
                if (log != null) log.Add(new LogEntry($"Content-Type: {contentType}", Colors.MediumTurquoise));
            }

            return this;
        }

        /// <summary>
        /// Performs a request.
        /// </summary>
        /// <param name="url">The URL</param>
        /// <param name="method">The HTTP method</param>
        /// <param name="log">The log (if any)</param>
        /// <returns>A 4-tuple containing Address, Response code, Headers and Cookies.</returns>
        public (string, string, Dictionary<string, string>, Dictionary<string, string>) Perform(string url, HttpMethod method, List<LogEntry> log = null)
        {
            var address = "";
            var responseCode = "0";
            var headers = new Dictionary<string, string>();
            var cookies = oldCookies;

            try
            {
                // Get response
                response = request.Raw(method, url, content);

                // Get address
                address = response.Address.ToString();
                if (log != null) log.Add(new LogEntry("Address: " + address, Colors.Cyan));

                // Get code
                responseCode = ((int)response.StatusCode).ToString();
                if (log != null) log.Add(new LogEntry($"Response code: {responseCode} ({response.StatusCode})", Colors.Cyan));

                // Get headers
                if (log != null) log.Add(new LogEntry("Received headers:", Colors.DeepPink));
                var headersList = new List<KeyValuePair<string, string>>();
                var receivedHeaders = response.EnumerateHeaders();
                while (receivedHeaders.MoveNext())
                {
                    var header = receivedHeaders.Current;
                    headersList.Add(new KeyValuePair<string, string>(header.Key, header.Value));
                    if (log != null) log.Add(new LogEntry($"{header.Key}: {header.Value}", Colors.LightPink));
                }
                headers = headersList.ToDictionary(h => h.Key, h => h.Value);
                hasContentLength = headers.ContainsKey("Content-Length");
                isGZipped = headers.ContainsKey("Content-Encoding") && headers["Content-Encoding"].Contains("gzip");

                // Get cookies
                cookies = response.Cookies;
                if (log != null)
                {
                    log.Add(new LogEntry("Received cookies:", Colors.Goldenrod));
                    foreach (var cookie in response.Cookies)
                    {
                        // If the cookie was already present before, don't log it
                        if (oldCookies.ContainsKey(cookie.Key) && oldCookies[cookie.Key] == cookie.Value) continue;
                        log.Add(new LogEntry($"{cookie.Key}: {cookie.Value}", Colors.LightGoldenrodYellow));
                    }
                }
            }
            catch (Exception ex)
            {
                if (log != null) log.Add(new LogEntry(ex.Message, Colors.White));
                
                if (ex.GetType() == typeof(HttpException))
                {
                    responseCode = ((HttpException)ex).HttpStatusCode.ToString();
                    if (log != null) log.Add(new LogEntry("Status code: " + responseCode, Colors.Cyan));
                }

                throw;
            }

            return (address, responseCode, headers, cookies);
        }

        /// <summary>
        /// Saves the response to a string.
        /// </summary>
        /// <param name="readResponseSource"></param>
        /// <param name="headers">The headers, to add the Content-Length header (if needed)</param>
        /// <param name="log">The log (if any)</param>
        /// <returns>The response source as a string</returns>
        public string SaveString(bool readResponseSource, Dictionary<string, string> headers = null, List<LogEntry> log = null)
        {
            var source = "";
            var responseString = response.ToString();

            if (log != null) log.Add(new LogEntry("Response Source:", Colors.Green));
            if (readResponseSource)
            {
                source = responseString;
                if (log != null) log.Add(new LogEntry(source, Colors.GreenYellow));
            }
            else
            {
                if (log != null) log.Add(new LogEntry("[SKIPPED]", Colors.GreenYellow));
            }

            if (!hasContentLength && headers != null)
            {
                if (responseString.Length == 0)
                {
                    headers["Content-Length"] = "0";
                }
                else
                {
                    if (isGZipped)
                    {
                        headers["Content-Length"] = GZip.Zip(responseString).Length.ToString();
                    }
                    else
                    {
                        headers["Content-Length"] = responseString.Length.ToString();
                    }
                }

                if (log != null) log.Add(new LogEntry($"Calculated header: Content-Length: {headers["Content-Length"]}", Colors.LightPink));
            }

            return source;
        }

        /// <summary>
        /// Reads the response to a MemoryStream.
        /// </summary>
        /// <returns>The MemoryStream</returns>
        public MemoryStream GetResponseStream()
        {
            return response.ToMemoryStream();
        }

        /// <summary>
        /// Saves the response content to a file.
        /// </summary>
        /// <param name="path">The file path on disk</param>
        /// <param name="log">The log (if any)</param>
        public void SaveFile(string path, List<LogEntry> log = null)
        {
            var dirName = Path.GetDirectoryName(path);
            if (dirName != string.Empty) dirName += Path.DirectorySeparatorChar.ToString();
            var fileName = Path.GetFileNameWithoutExtension(path);
            var fileExtension = Path.GetExtension(path);
            var sanitizedPath = $"{dirName}{Files.Files.MakeValidFileName(fileName)}{fileExtension}";
            using (var stream = File.Create(sanitizedPath)) { response.ToMemoryStream().CopyTo(stream); } // Read the stream
            if (log != null) log.Add(new LogEntry("File saved as " + sanitizedPath, Colors.Green));
        }

        /// <summary>
        /// Generates a random string to be used for boundary.
        /// </summary>
        /// <returns>The random 16-character string</returns>
        internal static string GenerateMultipartBoundary()
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < 16; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            return $"------WebKitFormBoundary{builder.ToString().ToLower()}";
        }
    }

    /// <summary>
    /// Represents a Multipart Content
    /// </summary>
    public struct MultipartContent
    {
        /// <summary>The type of multipart content.</summary>
        public MultipartContentType Type;

        /// <summary>The name of the multipart content.</summary>
        public string Name;

        /// <summary>The value of the multipart content (a string value or a file path).</summary>
        public string Value;

        /// <summary>The Content-Type of the file content.</summary>
        public string ContentType;
    }
}
