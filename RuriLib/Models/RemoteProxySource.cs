using Extreme.Net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuriLib.Models
{
    /// <summary>
    /// A remote source where proxies can be downloaded and parsed from.
    /// </summary>
    public class RemoteProxySource
    {
        /// <summary>
        /// Creates a RemoteProxySource given a unique ID.
        /// </summary>
        /// <param name="id">The unique ID</param>
        public RemoteProxySource(int id)
        {
            Id = id;
        }

        /// <summary>The unique ID of the RemoteProxySource.</summary>
        public int Id { get; set; } = 0;

        /// <summary>Whether the RemoteProxySource is active or not.</summary>
        public bool Active { get; set; } = true;

        /// <summary>The Url of the remote source.</summary>
        public string Url { get; set; } = "";

        /// <summary>The type of downloaded proxies.</summary>
        public ProxyType Type { get; set; } = ProxyType.Http;

        /// <summary>
        /// Whether the proxy type has been initialized.
        /// </summary>
        [JsonIgnore]
        public bool TypeInitialized { get; set; } = false;

        /// <summary>The regex pattern to match when parsing proxies from the downloaded page. Must use capture groups.</summary>
        // Best Regex = @"((([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5]):[0-9]+)";
        public string Pattern { get; set; } = @"([0-9]+\.[0-9]+\.[0-9]+\.[0-9]+:[0-9]+)";

        /// <summary>The output format of the groups matched by Regex.</summary>
        public string Output { get; set; } = "[1]";
    }

    /// <summary>
    /// The result for async download of proxies from remote sources.
    /// </summary>
    public class RemoteProxySourceResult
    {
        /// <summary>Whether the download was successful.</summary>
        public bool Successful { get; set; }

        /// <summary>The Message of the Exception thrown while downloading or parsing the proxies.</summary>
        public string Error { get; set; }

        /// <summary>The Url from which the proxies were downloaded from.</summary>
        public string Url { get; set; }

        /// <summary>The downloaded and parsed proxies.</summary>
        public List<CProxy> Proxies { get; set; }
    }
}
