using Extreme.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuriLib.Functions.Download
{
    public static class Download
    {
        public static void ImageToFile(string fileName, BotData data, string localUrl, string userAgent = "")
        {
            HttpRequest request = new HttpRequest();

            if (userAgent != "") request.UserAgent = userAgent;
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
