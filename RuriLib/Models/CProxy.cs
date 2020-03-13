using Extreme.Net;
using LiteDB;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace RuriLib.Models
{
    /// <summary>
    /// The Status of a CProxy.
    /// </summary>
    public enum Status
    {
        /// <summary>The proxy is not assigned to any bot.</summary>
        AVAILABLE,

        /// <summary>The proxy is assigned to at least 1 bot.</summary>
        BUSY,

        /// <summary>The proxy didn't accept the connection.</summary>
        BAD,

        /// <summary>The proxy was banned.</summary>
        BANNED
    }

    /// <summary>
    /// The Working Status of a CProxy.
    /// </summary>
    public enum ProxyWorking
    {
        /// <summary>The proxy is working fine.</summary>
        YES,

        /// <summary>The proxy is down or not accepting connections.</summary>
        NO,

        /// <summary>The proxy was not tested.</summary>
        UNTESTED
    }

    /// <summary>
    /// A proxy that supports http(s) and socks4/4a/5 protocols, authorization and chaining.
    /// </summary>
    public class CProxy : Persistable<Guid>
    {
        private string proxy = "";
        /// <summary>The unparsed proxy string.</summary>
        public string Proxy { get => proxy; set { proxy = value; OnPropertyChanged(); } }

        private string username = "";
        /// <summary>The username used for authentication (empty if none).</summary>
        public string Username { get => username; set { username = value; OnPropertyChanged(); } }

        private string password = "";
        /// <summary>The password used for authentication (empty if none).</summary>
        public string Password { get => password; set { password = value; OnPropertyChanged(); } }

        private ProxyType type = ProxyType.Http;
        /// <summary>The type of proxy.</summary>
        public ProxyType Type { get => type; set { type = value; OnPropertyChanged(); } }

        private string country = "";
        /// <summary>The country of the proxy's ip.</summary>
        public string Country { get => country; set { country = value; OnPropertyChanged(); } }

        private int ping = 0;
        /// <summary>The response delay of the proxy.</summary>
        public int Ping { get => ping; set { ping = value; OnPropertyChanged(); } }

        /// <summary>The next proxy object in a Proxy Chain.</summary>
        public CProxy Next { get; set; } = null;

        private DateTime lastUsed = new DateTime(1970, 1, 1);
        /// <summary>When the proxy was last used.</summary>
        public DateTime LastUsed { get => lastUsed; set { lastUsed = value; OnPropertyChanged(); } }

        private DateTime lastChecked = new DateTime(1970, 1, 1);
        /// <summary>When the proxy was last checked.</summary>
        public DateTime LastChecked { get => lastChecked; set { lastChecked = value; OnPropertyChanged(); } }

        private ProxyWorking working = ProxyWorking.UNTESTED;
        /// <summary>The Working Status of the proxy.</summary>
        public ProxyWorking Working { get => working; set { working = value; OnPropertyChanged(); } }

        /// <summary>Whether the proxy has a successor in the Proxy Chain.</summary>
        [BsonIgnore]
        public bool HasNext { get { return Next != null; } }

        /// <summary>The number of times the proxy was used for a check.</summary>
        [BsonIgnore]
        public int Uses { get; set; } = 0;

        /// <summary>The number of bots the proxy is hooked to.</summary>
        [BsonIgnore]
        public int Hooked { get; set; } = 0;

        /// <summary>The clearance cookie from Cloudflare.</summary>
        [BsonIgnore]
        public string Clearance { get; set; } = "";

        /// <summary>The cfduid cookie from Cloudflare.</summary>
        [BsonIgnore]
        public string Cfduid { get; set; } = "";

        /// <summary>The status of the proxy.</summary>
        [BsonIgnore]
        public Status Status { get; set; } = Status.AVAILABLE;

        /// <summary>Needed for NoSQL deserialization.</summary>
        public CProxy() { }

        /// <summary>
        /// Creates a proxy given a string and a proxy type.
        /// </summary>
        /// <param name="proxy">The unparsed proxy string</param>
        /// <param name="type">The proxy type</param>
        public CProxy(string proxy, ProxyType type)
        {
            Proxy = proxy;
            Type = type;
        }

        /// <summary>
        /// Parses a CProxy object from an advanced string. Supports Proxy Chains.
        /// </summary>
        /// <param name="proxy">The string to parse the proxy from</param>
        /// <param name="defaultType">The default type to use when not specified</param>
        /// <param name="defaultUsername">The default username to use when not specified</param>
        /// <param name="defaultPassword">The default password to use when not specified</param>
        /// <returns>The parsed CProxy object</returns>
        public CProxy Parse(string proxy, ProxyType defaultType = ProxyType.Http, string defaultUsername = "", string defaultPassword = "")
        {
            // Take the first proxy of the chain
            var chain = proxy.Split(new string[] { "->" }, 2, StringSplitOptions.None);
            var current = chain[0];

            // If the type was specified, parse it and remove it from the string
            if (current.StartsWith("("))
            {
                var groups = Regex.Match(current, @"^\((.*)\)(.*)").Groups;
                Enum.TryParse<ProxyType>(groups[1].Value, true, out var type);
                Type = type;
                current = current.Replace($"({groups[1].Value})", "");
            }
            else // Otherwise set the default type passed
            {
                Type = defaultType;
            }

            // Split the host:port:user:pass portion and set the first two
            var fields = current.Split(':');
            Proxy = $"{fields[0]}:{fields[1]}";

            // Set the other two if they are present
            if (fields.Count() > 2)
            {
                Username = fields[2];
                Password = fields[3];
            }
            else
            {
                Username = defaultUsername;
                Password = defaultPassword;
            }

            // If the chain is not finished, set the next proxy by parsing recursively
            if (chain.Count() > 1)
            {
                Next = (new CProxy()).Parse(chain[1], defaultType, defaultUsername, defaultPassword);
            }

            // Finally return this proxy
            return this;
        }

        /// <summary>
        /// Gets the ProxyClient related to the specific proxy type.
        /// </summary>
        /// <returns>The ProxyClient to be used in a HttpRequest</returns>
        public ProxyClient GetClient()
        {
            if (HasNext)
            {
                return GetChainClient();
            }
            else
            {
                return GetStandardClient();
            }
        }

        /// <summary>
        /// Gets the standard ProxyClient related to the specific proxy type.
        /// </summary>
        /// <returns>The standard ProxyClient (not chained)</returns>
        private ProxyClient GetStandardClient()
        {
            ProxyClient pc;

            switch (Type)
            {
                case ProxyType.Http:
                    pc = HttpProxyClient.Parse(Proxy);
                    break;

                case ProxyType.Socks4:
                    pc = Socks4ProxyClient.Parse(Proxy);
                    break;

                case ProxyType.Socks4a:
                    pc = Socks4aProxyClient.Parse(Proxy);
                    break;

                case ProxyType.Socks5:
                    pc = Socks5ProxyClient.Parse(Proxy);
                    break;

                default:
                    return null;
            }

            pc.Username = Username;
            pc.Password = Password;

            return pc;
        }

        private ChainProxyClient GetChainClient()
        {
            ChainProxyClient cpc = new ChainProxyClient();

            var current = this;

            while (current != null)
            {
                cpc.AddProxy(current.GetStandardClient());
                current = current.Next;
            }

            return cpc;
        }

        /// <summary>The Host string parsed from the proxy.</summary>
        [BsonIgnore]
        public string Host
        {
            get
            {
                try { return Proxy.Split(':').First(); }
                catch { return ""; }
            }
        }

        /// <summary>The Port string parsed from the proxy.</summary>
        [BsonIgnore]
        public string Port
        {
            get
            {
                try { return Proxy.Split(':').Last(); }
                catch { return ""; }
            }
        }

        /// <summary>Whether the proxy is a valid numeric proxy.</summary>
        [BsonIgnore]
        public bool IsValidNumeric
        {
            get
            {
                return !(
                    Host == string.Empty ||
                    Port == string.Empty ||
                    Port.Any(c => !char.IsDigit(c)) ||
                    Host.Split('.').Count() != 4 ||
                    !IsNumeric
                    );
            }
        }

        /// <summary>Whether the proxy is numeric.</summary>
        [BsonIgnore]
        public bool IsNumeric { get { return !Host.Split('.').Any(x => x.Any(c => !char.IsDigit(c))); } }

        /// <summary>How long ago the proxy was used.</summary>
        [BsonIgnore]
        public TimeSpan UsedAgo { get { return DateTime.Now - LastUsed; } }
    }
}
