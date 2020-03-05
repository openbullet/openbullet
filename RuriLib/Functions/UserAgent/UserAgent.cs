using Extreme.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuriLib.Functions.UserAgent
{
    /// <summary>
    /// Provides methods to generate User Agents.
    /// </summary>
    public static class UserAgent
    {
        /// <summary>
        /// Enumerates browsers for which a User Agent can be generated.
        /// </summary>
        public enum Browser
        {
            /// <summary>The Google Chrome browser.</summary>
            Chrome,

            /// <summary>The Mozilla Firefox browser.</summary>
            Firefox,

            /// <summary>The Internet Explorer browser.</summary>
            InternetExplorer,

            /// <summary>The Opera browser.</summary>
            Opera,

            /// <summary>The Opera Mini mobile browser.</summary>
            OperaMini
        }

        /// <summary>
        /// Generates a User Agent for a specific Browser.
        /// </summary>
        /// <param name="browser">The Browser</param>
        /// <returns>A User Agent for the given browser</returns>
        public static string ForBrowser(Browser browser)
        {
            switch (browser)
            {
                case Browser.Chrome:
                    return Http.ChromeUserAgent();

                case Browser.Firefox:
                    return Http.FirefoxUserAgent();

                case Browser.InternetExplorer:
                    return Http.IEUserAgent();

                case Browser.Opera:
                    return Http.OperaUserAgent();

                case Browser.OperaMini:
                    return Http.OperaMiniUserAgent();

                default:
                    throw new Exception("Browser not supported");
            }
        }

        /// <summary>
        /// Gets a random User-Agent header.
        /// </summary>
        /// <param name="rand">A random number generator</param>
        /// <returns>A randomly generated User-Agent header</returns>
        public static string Random(Random rand)
        {
            // All credits for this method go to the Leaf.xNet fork of Extreme.NET
            // https://github.com/csharp-leaf/Leaf.xNet

            int random = rand.Next(99) + 1;

            // Chrome = 70%
            if (random >= 1 && random <= 70)
                return Http.ChromeUserAgent();

            // Firefox = 15%
            if (random > 70 && random <= 85)
                return Http.FirefoxUserAgent();

            // IE = 6%
            if (random > 85 && random <= 91)
                return Http.IEUserAgent();

            // Opera 12 = 5%
            if (random > 91 && random <= 96)
                return Http.OperaUserAgent();

            // Opera mini = 4%
            return Http.OperaMiniUserAgent();
        }
    }
}
