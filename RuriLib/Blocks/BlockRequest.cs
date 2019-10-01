using Extreme.Net;
using RuriLib.LS;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace RuriLib
{
    /// <summary>
    /// The types of request that can be performed.
    /// </summary>
    public enum RequestType
    {
        /// <summary>A standard request with standard content.</summary>
        Standard,

        /// <summary>A request which uses the 'Authentication: Basic' header.</summary>
        BasicAuth,

        /// <summary>A request which contains multipart content (strings and/or files).</summary>
        Multipart
    }

    /// <summary>
    /// The available types of multipart contents.
    /// </summary>
    public enum MultipartContentType
    {
        /// <summary>A string content.</summary>
        String,

        /// <summary>A file content.</summary>
        File
    }

    /// <summary>
    /// The type of data expected inside the HTTP response.
    /// </summary>
    public enum ResponseType
    {
        /// <summary>A string response, e.g. an HTML page.</summary>
        String,

        /// <summary>A file response, e.g. an image.</summary>
        File
    }

    /// <summary>
    /// A block that can perform HTTP requests.
    /// </summary>
    public class BlockRequest : BlockBase
    {
        #region Variables
        private string url = "https://google.com";
        /// <summary>The URL to call, including additional GET query parameters.</summary>
        public string Url { get { return url; } set { url = value; OnPropertyChanged(); } }

        private RequestType requestType = RequestType.Standard;
        /// <summary>The request type.</summary>
        public RequestType RequestType { get { return requestType; } set { requestType = value; OnPropertyChanged(); } }

        // Basic Auth
        private string authUser = "";
        /// <summary>The username for basic auth requests.</summary>
        public string AuthUser { get { return authUser; } set { authUser = value; OnPropertyChanged(); } }

        private string authPass = "";
        /// <summary>The password for basic auth requests.</summary>
        public string AuthPass { get { return authPass; } set { authPass = value; OnPropertyChanged(); } }

        // Standard
        private string postData = "";
        /// <summary>The content of the request, sent after the headers. Use '\n' to input a linebreak.</summary>
        public string PostData { get { return postData; } set { postData = value; OnPropertyChanged(); } }

        private HttpMethod method = HttpMethod.GET;
        /// <summary>The method of the HTTP request.</summary>
        public HttpMethod Method { get { return method; } set { method = value; OnPropertyChanged(); } }

        /// <summary>The custom headers that are sent in the HTTP request.</summary>
        public Dictionary<string, string> CustomHeaders { get; set; } = new Dictionary<string, string>() {
            { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko" },
            { "Pragma", "no-cache" },
            { "Accept", "*/*" }
        };

        /// <summary>The custom cookies that are sent in the HTTP request.</summary>
        public Dictionary<string, string> CustomCookies { get; set; } = new Dictionary<string, string>() { };

        private string contentType = "application/x-www-form-urlencoded";
        /// <summary>The type of content the server should expect.</summary>
        public string ContentType { get { return contentType; } set { contentType = value; OnPropertyChanged(); } }

        private bool autoRedirect = true;
        /// <summary>Whether to perform automatic redirection in the case of 3xx headers.</summary>
        public bool AutoRedirect { get { return autoRedirect; } set { autoRedirect = value; OnPropertyChanged(); } }

        private bool readResponseSource = true;
        /// <summary>Whether to read the stream of data from the HTTP response. Set to false if only the headers are needed, in order to speed up the process.</summary>
        public bool ReadResponseSource { get { return readResponseSource; } set { readResponseSource = value; OnPropertyChanged(); } }

        private bool parseQuery = false;
        /// <summary>Whether to parse the GET parameters manually (fixes Extreme.NET issues on some websites).</summary>
        public bool ParseQuery { get { return parseQuery; } set { parseQuery = value; OnPropertyChanged(); } }

        private bool encodeContent = false;
        /// <summary>Whether to URL encode the content before sending it.</summary>
        public bool EncodeContent { get { return encodeContent; } set { encodeContent = value; OnPropertyChanged(); } }

        private bool acceptEncoding = true;
        /// <summary>Whether to automatically generate an Accept-Encoding header.</summary>
        public bool AcceptEncoding { get { return acceptEncoding; } set { acceptEncoding = value; OnPropertyChanged(); } }

        // Multipart
        private string multipartBoundary = "";
        /// <summary>The boundary that separates multipart contents.</summary>
        public string MultipartBoundary { get { return multipartBoundary; } set { multipartBoundary = value; OnPropertyChanged(); } }

        /// <summary>The list of contents to send in a multipart request.</summary>
        public List<MultipartContent> MultipartContents { get; set; } = new List<MultipartContent>();

        private ResponseType responseType = ResponseType.String;
        /// <summary>The type of response expected from the server.</summary>
        public ResponseType ResponseType { get { return responseType; } set { responseType = value; OnPropertyChanged(); } }

        private string downloadPath = "";
        /// <summary>The path of the file where a FILE response needs to be stored.</summary>
        public string DownloadPath { get { return downloadPath; } set { downloadPath = value; OnPropertyChanged(); } }

        private bool saveAsScreenshot = false;
        /// <summary>Whether to add the downloaded image to the default screenshot path.</summary>
        public bool SaveAsScreenshot { get { return saveAsScreenshot; } set { saveAsScreenshot = value; OnPropertyChanged(); } }
        #endregion

        /// <summary>
        /// Creates a Request block.
        /// </summary>
        public BlockRequest()
        {
            Label = "REQUEST";
        }

        /// <inheritdoc />
        public override BlockBase FromLS(string line)
        {
            // Trim the line
            var input = line.Trim();

            // Parse the label
            if (input.StartsWith("#"))
                Label = LineParser.ParseLabel(ref input);

            Method = (HttpMethod)LineParser.ParseEnum(ref input, "METHOD", typeof(HttpMethod));
            Url = LineParser.ParseLiteral(ref input, "URL");

            while (LineParser.Lookahead(ref input) == TokenType.Boolean)
                LineParser.SetBool(ref input, this);

            CustomHeaders.Clear(); // Remove the default headers

            while (input != "" && !input.StartsWith("->"))
            {
                var parsed = LineParser.ParseToken(ref input, TokenType.Parameter, true).ToUpper();
                switch (parsed)
                {
                    case "MULTIPART":
                        RequestType = RequestType.Multipart;
                        break;

                    case "BASICAUTH":
                        RequestType = RequestType.BasicAuth;
                        break;

                    case "STANDARD":
                        RequestType = RequestType.Standard;
                        break;

                    case "CONTENT":
                        PostData = LineParser.ParseLiteral(ref input, "POST DATA");
                        break;

                    case "STRINGCONTENT":
                        var stringContentPair = ParseString(LineParser.ParseLiteral(ref input, "STRING CONTENT"), ':', 2);
                        MultipartContents.Add(new MultipartContent() { Type = MultipartContentType.String, Name = stringContentPair[0], Value = stringContentPair[1] });
                        break;

                    case "FILECONTENT":
                        var fileContentTriplet = ParseString(LineParser.ParseLiteral(ref input, "FILE CONTENT"), ':', 3);
                        MultipartContents.Add(new MultipartContent() { Type = MultipartContentType.File, Name = fileContentTriplet[0], Value = fileContentTriplet[1], ContentType = fileContentTriplet[2] });
                        break;

                    case "COOKIE":
                        var cookiePair = ParseString(LineParser.ParseLiteral(ref input, "COOKIE VALUE"), ':', 2);
                        CustomCookies[cookiePair[0]] = cookiePair[1];
                        break;

                    case "HEADER":
                        var headerPair = ParseString(LineParser.ParseLiteral(ref input, "HEADER VALUE"), ':', 2);
                        CustomHeaders[headerPair[0]] = headerPair[1];
                        break;

                    case "CONTENTTYPE":
                        ContentType = LineParser.ParseLiteral(ref input, "CONTENT TYPE");
                        break;

                    case "USERNAME":
                        AuthUser = LineParser.ParseLiteral(ref input, "USERNAME");
                        break;

                    case "PASSWORD":
                        AuthPass = LineParser.ParseLiteral(ref input, "PASSWORD");
                        break;

                    case "BOUNDARY":
                        MultipartBoundary = LineParser.ParseLiteral(ref input, "BOUNDARY");
                        break;

                    default:
                        break;
                }
            }

            if (input.StartsWith("->"))
            {
                LineParser.EnsureIdentifier(ref input, "->");
                var outType = LineParser.ParseToken(ref input, TokenType.Parameter, true);
                if (outType.ToUpper() == "STRING") ResponseType = ResponseType.String;
                else if (outType.ToUpper() == "FILE")
                {
                    ResponseType = ResponseType.File;
                    DownloadPath = LineParser.ParseLiteral(ref input, "DOWNLOAD PATH");
                    while (LineParser.Lookahead(ref input) == TokenType.Boolean)
                    {
                        LineParser.SetBool(ref input, this);
                    }
                }
            }

            return this;
        }

        /// <summary>
        /// Parses values from a string.
        /// </summary>
        /// <param name="input">The string to parse</param>
        /// <param name="separator">The character that separates the elements</param>
        /// <param name="count">The number of elements to return</param>
        /// <returns>The array of the parsed elements.</returns>
        public static string[] ParseString(string input, char separator, int count)
        {
            return input.Split(new[] { separator }, count).Select(s => s.Trim()).ToArray();
        }

        /// <inheritdoc />
        public override string ToLS(bool indent = true)
        {
            var writer = new BlockWriter(GetType(), indent, Disabled);
            writer
                .Label(Label)
                .Token("REQUEST")
                .Token(Method)
                .Literal(Url)
                .Boolean(AcceptEncoding, "AcceptEncoding")
                .Boolean(AutoRedirect, "AutoRedirect")
                .Boolean(ReadResponseSource, "ReadResponseSource")
                .Boolean(ParseQuery, "ParseQuery")
                .Boolean(EncodeContent, "EncodeContent")
                .Token(RequestType, "RequestType")
                .Indent();

            switch (RequestType)
            {
                case RequestType.BasicAuth:
                    writer
                        .Token("USERNAME")
                        .Literal(AuthUser)
                        .Token("PASSWORD")
                        .Literal(AuthPass)
                        .Indent();
                    break;

                case RequestType.Standard:
                    if (CanContainBody(method))
                    {
                        writer
                            .Token("CONTENT")
                            .Literal(PostData)
                            .Indent()
                            .Token("CONTENTTYPE")
                            .Literal(ContentType);
                    }
                    break;

                case RequestType.Multipart:
                    foreach(var c in MultipartContents)
                    {
                        writer
                            .Indent()
                            .Token($"{c.Type.ToString().ToUpper()}CONTENT");

                        if (c.Type == MultipartContentType.String)
                        {
                            writer.Literal($"{c.Name}: {c.Value}");
                        }
                        else if (c.Type == MultipartContentType.File)
                        {
                            writer.Literal($"{c.Name}: {c.Value}: {c.ContentType}");
                        }
                    }
                    if (!writer.CheckDefault(MultipartBoundary, "MultipartBoundary"))
                    {
                        writer
                            .Indent()
                            .Token("BOUNDARY")
                            .Literal(MultipartBoundary);
                    }
                    break;
            }

            foreach (var c in CustomCookies)
            {
                writer
                    .Indent()
                    .Token("COOKIE")
                    .Literal($"{c.Key}: {c.Value}");
            }

            foreach (var h in CustomHeaders)
            {
                writer
                    .Indent()
                    .Token("HEADER")
                    .Literal($"{h.Key}: {h.Value}");
            }

            if (ResponseType == ResponseType.File)
            {
                writer
                    .Indent()
                    .Arrow()
                    .Token("FILE")
                    .Literal(DownloadPath)
                    .Boolean(SaveAsScreenshot, "SaveAsScreenshot");
            }

            return writer.ToString();
        }

        /// <inheritdoc />
        public override void Process(BotData data)
        {
            base.Process(data);

            #region Request
            // Set base URL
            var localUrl = ReplaceValues(url, data);
            var cType = ReplaceValues(contentType, data);
            var oldJar = data.Cookies;

            // Create request
            HttpRequest request = new HttpRequest();

            // Setup options
            var timeout = data.GlobalSettings.General.RequestTimeout * 1000;
            request.IgnoreProtocolErrors = true;
            request.AllowAutoRedirect = autoRedirect;
            request.EnableEncodingContent = acceptEncoding;
            request.ReadWriteTimeout = timeout;
            request.ConnectTimeout = timeout;
            request.KeepAlive = true;
            request.MaximumAutomaticRedirections = data.ConfigSettings.MaxRedirects;

            // Check if it has GET parameters
            if (ParseQuery && localUrl.Contains('?') && localUrl.Contains('='))
            {
                // Remove the query from the base URL
                localUrl = ReplaceValues(url.Split('?')[0], data);
                data.Log(new LogEntry($"Calling Base URL: {localUrl}", Colors.MediumTurquoise));

                // Parse the GET parameters
                var getParams = ReplaceValues(url.Split('?')[1], data);
                var paramList = getParams.Split('&');

                // Build the query, first replace variables in them and encode the parameters
                foreach (var par in paramList)
                {
                    var split = par.Split('=');

                    // Encode them if needed
                    if (split[0].Contains('%')) split[0] = Uri.EscapeDataString(split[0]);
                    if (split[1].Contains('%')) split[1] = Uri.EscapeDataString(split[1]);

                    // Add them to the query
                    request.AddUrlParam(split[0], split[1]);

                    data.Log(new LogEntry($"Added Query Parameter: {split[0]} = {split[1]}", Colors.MediumTurquoise));
                }
            }
            else
            {
                data.Log(new LogEntry($"Calling URL: {localUrl}", Colors.MediumTurquoise));
            }

            // Set up the Content and Content-Type
            HttpContent content = null;
            switch (requestType)
            {
                case RequestType.Standard:
                    var pData = ReplaceValues(Regex.Replace(postData, @"(?<!\\)\\n", Environment.NewLine).Replace(@"\\n", @"\n"), data);

                    if (CanContainBody(method))
                    {
                        if (encodeContent)
                        {
                            // Very dirty but it works
                            var nonce = data.Random.Next(1000000, 9999999);
                            pData = pData.Replace("&", $"{nonce}&{nonce}").Replace("=", $"{nonce}={nonce}");
                            pData = System.Uri.EscapeDataString(pData).Replace($"{nonce}%26{nonce}", "&").Replace($"{nonce}%3D{nonce}", "=");
                        }

                        content = new StringContent(pData);
                        content.ContentType = cType;
                        data.Log(new LogEntry($"Post Data: {pData}", Colors.MediumTurquoise));
                    }
                    break;

                case RequestType.Multipart:
                    var bdry = multipartBoundary != "" ? ReplaceValues(multipartBoundary, data) : GenerateMultipartBoundary();
                    content = new Extreme.Net.MultipartContent(bdry);
                    var mContent = content as Extreme.Net.MultipartContent;
                    data.Log(new LogEntry($"Content-Type: multipart/form-data; boundary={bdry}", Colors.MediumTurquoise));
                    data.Log(new LogEntry("Multipart Data:", Colors.MediumTurquoise));
                    data.Log(new LogEntry(bdry, Colors.MediumTurquoise));
                    foreach (var c in MultipartContents)
                    {
                        var rValue = ReplaceValues(c.Value, data);
                        var rName = ReplaceValues(c.Name, data);
                        var rContentType = ReplaceValues(c.ContentType, data);

                        if (c.Type == MultipartContentType.String)
                        {
                            mContent.Add(new StringContent(rValue), rName);
                            data.Log(new LogEntry($"Content-Disposition: form-data; name=\"{rName}\"{Environment.NewLine}{Environment.NewLine}{rValue}", Colors.MediumTurquoise));
                        }
                        else if (c.Type == MultipartContentType.File)
                        {
                            mContent.Add(new FileContent(rValue), rName, rValue, rContentType);
                            data.Log(new LogEntry($"Content-Disposition: form-data; name=\"{rName}\"; filename=\"{rValue}\"{Environment.NewLine}Content-Type: {rContentType}{Environment.NewLine}{Environment.NewLine}[FILE CONTENT OMITTED]", Colors.MediumTurquoise));
                        }
                        data.Log(new LogEntry(bdry, Colors.MediumTurquoise));
                    }
                    break;

                default:
                    break;
            }

            // Set proxy
            if (data.UseProxies)
            {
                request.Proxy = data.Proxy.GetClient();

                try
                {
                    request.Proxy.ReadWriteTimeout = timeout;
                    request.Proxy.ConnectTimeout = timeout;
                    request.Proxy.Username = data.Proxy.Username;
                    request.Proxy.Password = data.Proxy.Password;
                }
                catch { }
            }

            // Set headers
            data.Log(new LogEntry("Sent Headers:", Colors.DarkTurquoise));
            // var fixedNames = Enum.GetNames(typeof(HttpHeader)).Select(n => n.ToLower());
            foreach (var header in CustomHeaders)
            {
                try
                {
                    var key = ReplaceValues(header.Key, data);
                    var replacedKey = key.Replace("-", "").ToLower(); // Used to compare with the HttpHeader enum
                    var val = ReplaceValues(header.Value, data);

                    if (replacedKey == "contenttype" && content != null) { continue; } // Disregard additional Content-Type headers
                    if (replacedKey == "acceptencoding" && acceptEncoding) { continue; } // Disregard additional Accept-Encoding headers
                    // else if (fixedNames.Contains(replacedKey)) request.AddHeader((HttpHeader)Enum.Parse(typeof(HttpHeader), replacedKey, true), val);
                    else request.AddHeader(key, val);

                    data.Log(new LogEntry(key + ": " + val, Colors.MediumTurquoise));
                }
                catch { }
            }

            // Add the authorization header on a Basic Auth request
            if (requestType == RequestType.BasicAuth)
            {
                var usr = ReplaceValues(authUser, data);
                var pwd = ReplaceValues(authPass, data);
                var auth = "Basic " + BlockFunction.Base64Encode(usr + ":" + pwd);
                request.AddHeader("Authorization", auth);
                data.Log(new LogEntry($"Authorization: {auth}", Colors.MediumTurquoise));
            }

            // Add the content-type header
            if (CanContainBody(method) && content != null && requestType == RequestType.Standard)
            {
                data.Log(new LogEntry($"Content-Type: {cType}", Colors.MediumTurquoise));
            }

            // Add new user-defined custom cookies to the bot's cookie jar
            request.Cookies = new CookieDictionary();
            foreach (var cookie in CustomCookies)
                data.Cookies[ReplaceValues(cookie.Key, data)] = ReplaceValues(cookie.Value, data);

            // Set cookies from the bot's cookie jar to the request's CookieDictionary
            data.Log(new LogEntry("Sent Cookies:", Colors.MediumTurquoise));
            foreach (var cookie in data.Cookies)
            {
                request.Cookies.Add(cookie.Key, cookie.Value);
                data.Log(new LogEntry($"{cookie.Key}: {cookie.Value}", Colors.MediumTurquoise));
            }

            data.LogNewLine();
            #endregion

            #region Response
            // Create the response
            HttpResponse response = null;
            
            try
            {
                // Get response
                response = request.Raw(method, localUrl, content);
                var responseString = "";

                // Get address
                data.Address = response.Address.ToString();
                data.Log(new LogEntry("Address: " + data.Address, Colors.Cyan));

                // Get code
                data.ResponseCode = ((int)response.StatusCode).ToString();
                data.Log(new LogEntry($"Response code: {data.ResponseCode} ({response.StatusCode})", Colors.Cyan));

                // Get headers
                data.Log(new LogEntry("Received headers:", Colors.DeepPink));
                var headerList = new List<KeyValuePair<string, string>>();
                var receivedHeaders = response.EnumerateHeaders();
                data.ResponseHeaders.Clear();
                while (receivedHeaders.MoveNext())
                {
                    var header = receivedHeaders.Current;
                    data.ResponseHeaders.Add(header.Key, header.Value);
                    data.Log(new LogEntry($"{header.Key}: {header.Value}", Colors.LightPink));
                }
                if (!response.ContainsHeader(HttpHeader.ContentLength) && ResponseType != ResponseType.File)
                {
                    responseString = response.ToString(); // Read the stream

                    if (data.ResponseHeaders.ContainsKey("Content-Encoding") && data.ResponseHeaders["Content-Encoding"].Contains("gzip"))
                    {
                        data.ResponseHeaders["Content-Length"] = GZip.Zip(responseString).Length.ToString();
                    }
                    else
                    {
                        data.ResponseHeaders["Content-Length"] = responseString.Length.ToString();
                    }

                    data.Log(new LogEntry($"Content-Length: {data.ResponseHeaders["Content-Length"]}", Colors.LightPink));
                }

                // Get cookies
                data.Log(new LogEntry("Received cookies:", Colors.Goldenrod));
                data.Cookies = response.Cookies;
                foreach (var cookie in response.Cookies)
                {
                    // If the cookie was already present before, don't log it
                    if (oldJar.ContainsKey(cookie.Key) && oldJar[cookie.Key] == cookie.Value) continue;

                    data.Log(new LogEntry($"{cookie.Key}: {cookie.Value}", Colors.LightGoldenrodYellow));
                }

                // Save the response content
                switch (responseType)
                {
                    case ResponseType.String:
                        data.Log(new LogEntry("Response Source:", Colors.Green));
                        if (readResponseSource)
                        {
                            if (responseString == "") responseString = response.ToString(); // Read the stream if you didn't already read it
                            data.ResponseSource = responseString;
                            data.Log(new LogEntry(data.ResponseSource, Colors.GreenYellow));
                        }
                        else
                        {
                            data.ResponseSource = "";
                            data.Log(new LogEntry("[SKIPPED]", Colors.GreenYellow));
                        }
                        break;

                    case ResponseType.File:
                        if (SaveAsScreenshot)
                        {
                            SaveScreenshot(response.ToMemoryStream(), data); // Read the stream
                            data.Log(new LogEntry("File saved as screenshot", Colors.Green));
                        }
                        else
                        {
                            var file = MakeValidFileName(ReplaceValues(downloadPath, data));
                            using (var stream = File.Create(file)) { response.ToMemoryStream().CopyTo(stream); } // Read the stream
                            data.Log(new LogEntry("File saved as " + file, Colors.Green));
                        }
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                data.Log(new LogEntry(ex.Message, Colors.White));
                if (ex.GetType() == typeof(HttpException))
                {
                    data.ResponseCode = ((HttpException)ex).HttpStatusCode.ToString();
                    data.Log(new LogEntry("Status code: " + data.ResponseCode, Colors.Cyan));
                }

                if (!data.ConfigSettings.IgnoreResponseErrors) throw;
            }
            #endregion
        }

        #region Custom Cookies, Headers and Multipart Contents
        /// <summary>
        /// Builds a string containing custom cookies.
        /// </summary>
        /// <returns>One cookie per line, with name and value separated by a colon</returns>
        public string GetCustomCookies()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var pair in CustomCookies)
            {
                sb.Append($"{pair.Key}: {pair.Value}");
                if (!pair.Equals(CustomCookies.Last())) sb.Append(Environment.NewLine);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Sets custom cookies from an array of lines.
        /// </summary>
        /// <param name="lines">The lines containing the colon-separated name and value of the cookies</param>
        public void SetCustomCookies(string[] lines)
        {
            CustomCookies.Clear();
            foreach (var line in lines)
            {
                if (line.Contains(':'))
                {
                    var split = line.Split(new[] { ':' }, 2);
                    CustomCookies[split[0].Trim()] = split[1].Trim();
                }
            }
        }

        /// <summary>
        /// Builds a string containing custom headers.
        /// </summary>
        /// <returns>One header per line, with name and value separated by a colon</returns>
        public string GetCustomHeaders()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var pair in CustomHeaders)
            {
                sb.Append($"{pair.Key}: {pair.Value}");
                if (!pair.Equals(CustomHeaders.Last())) sb.Append(Environment.NewLine);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Sets custom headers from an array of lines.
        /// </summary>
        /// <param name="lines">The lines containing the colon-separated name and value of the headers</param>
        public void SetCustomHeaders(string[] lines)
        {
            CustomHeaders.Clear();
            foreach (var line in lines)
            {
                if (line.Contains(':'))
                {
                    var split = line.Split(new[] { ':' }, 2);
                    CustomHeaders[split[0].Trim()] = split[1].Trim();
                }
            }
        }

        /// <summary>
        /// Builds a string containing multipart content.
        /// </summary>
        /// <returns>One content per line, with type, name and value separated by a colon</returns>
        public string GetMultipartContents()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var c in MultipartContents)
            {
                sb.Append($"{c.Type.ToString().ToUpper()}: {c.Name}: {c.Value}");
                if (!c.Equals(MultipartContents.Last())) sb.Append(Environment.NewLine);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Sets multipart contents from an array of lines.
        /// </summary>
        /// <param name="lines">The lines containing the colon-separated type, name and value of the multipart contents</param>
        public void SetMultipartContents(string[] lines)
        {
            MultipartContents.Clear();
            foreach(var line in lines)
            {
                try
                {
                    var split = line.Split(new[] { ':' }, 3);
                    MultipartContents.Add(new MultipartContent() {
                        Type = (MultipartContentType)Enum.Parse(typeof(MultipartContentType), split[0].Trim(), true),
                        Name = split[1].Trim(),
                        Value = split[2].Trim()
                    });
                }
                catch { }
            }
        }

        #endregion

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

        private static bool CanContainBody(HttpMethod method)
        {
            return method == HttpMethod.POST || method == HttpMethod.PUT || method == HttpMethod.DELETE;
        }
    }

    /// <summary>
    /// Represents a Multipart Content
    /// </summary>
    public class MultipartContent
    {
        /// <summary>The type of multipart content.</summary>
        public MultipartContentType Type { get; set; } = MultipartContentType.String;

        /// <summary>The name of the multipart content.</summary>
        public string Name { get; set; } = "";

        /// <summary>The value of the multipart content (a string value or a file path).</summary>
        public string Value { get; set; } = "";

        /// <summary>The Content-Type of the file content.</summary>
        public string ContentType { get; set; } = "";
    }
}
