using Extreme.Net;
using RuriLib.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuriLib.Functions.Download
{
    /// <summary>
    /// Provides methods to download files from the internet.
    /// </summary>
    public static class Download
    {
        /// <summary>
        /// Downloads a remote file.
        /// </summary>
        /// <param name="fileName">The destination file on disk</param>
        /// <param name="url">The URL of the remote file</param>
        /// <param name="useProxies">Whether to use proxies for the request</param>
        /// <param name="proxy">The proxy, if needed</param>
        /// <param name="cookies">The cookies to use in the request</param>
        /// <param name="newCookies">The new cookie dictionary containing the new cookies too</param>
        /// <param name="timeout">The request timeout in milliseconds</param>
        /// <param name="userAgent">The user agent to use</param>
        public static void RemoteFile(
            string fileName, string url, bool useProxies, CProxy proxy, Dictionary<string, string> cookies, out Dictionary<string, string> newCookies, int timeout, string userAgent = "")
        {
            HttpRequest request = new HttpRequest();

            if (userAgent != string.Empty) request.UserAgent = userAgent;
            request.Cookies = new CookieDictionary();
            foreach (var cookie in cookies)
                request.Cookies.Add(cookie.Key, cookie.Value);

            // Set proxy
            if (useProxies)
            {
                request.Proxy = proxy.GetClient();
                request.Proxy.ReadWriteTimeout = timeout;
                request.Proxy.ConnectTimeout = timeout;
                request.Proxy.Username = proxy.Username;
                request.Proxy.Password = proxy.Password;
            }

            HttpResponse response = request.Get(url);

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

            newCookies = response.Cookies;
        }
    }
}
