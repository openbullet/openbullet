using Extreme.Net;

namespace RuriLib.ViewModels
{
    /// <summary>
    /// The Source of proxies that gets queried when all proxies are banned in order to get fresh ones.
    /// </summary>
    public enum ProxyReloadSource
    {
        /// <summary>The ProxyManager.</summary>
        Manager,
        /// <summary>A file on the disk.</summary>
        File,
        /// <summary>An online API.</summary>
        API
    }

    /// <summary>
    /// Provides proxy-related settings.
    /// </summary>
    public class SettingsProxies : ViewModelBase
    {
        #region General
        private bool concurrentUse = false;
        /// <summary>Whether to allow two Bots to use the same proxy.</summary>
        public bool ConcurrentUse { get { return concurrentUse; } set { concurrentUse = value; OnPropertyChanged(); } }

        private bool neverBan = false;
        /// <summary>Whether to never ban the proxies.</summary>
        public bool NeverBan { get { return neverBan; } set { neverBan = value; OnPropertyChanged(); } }

        private bool banAfterGoodStatus = false;
        /// <summary>Whether to never ban the proxy after a SUCCESS, CUSTOM or NONE status (not FAIL or RETRY).</summary>
        public bool BanAfterGoodStatus { get { return banAfterGoodStatus; } set { banAfterGoodStatus = value; OnPropertyChanged(); } }

        private bool shuffleOnStart = false;
        /// <summary>Whether proxy lists should be shuffled before being assigned to the Runner.</summary>
        public bool ShuffleOnStart { get { return shuffleOnStart; } set { shuffleOnStart = value; OnPropertyChanged(); } }
        #endregion

        #region Reload
        private bool reload = true;
        /// <summary>Whether to reload the proxies from the proxy source after they are all banned.</summary>
        public bool Reload { get { return reload; } set { reload = value; OnPropertyChanged(); } }

        private ProxyReloadSource reloadSource = ProxyReloadSource.Manager;
        /// <summary>The source to reload the proxies from.</summary>
        public ProxyReloadSource ReloadSource { get { return reloadSource; } set { reloadSource = value; OnPropertyChanged(); } }

        private string reloadPath = "";
        /// <summary>The API URL or the file path on disk.</summary>
        public string ReloadPath { get { return reloadPath; } set { reloadPath = value; OnPropertyChanged(); } }

        private ProxyType reloadType = ProxyType.Http;
        /// <summary>The Type of the proxies to load.</summary>
        public ProxyType ReloadType { get { return reloadType; } set { reloadType = value; OnPropertyChanged(); } }

        private bool parseWithIPRegex = false;
        /// <summary>
        /// <para>Whether to use an IP:PORT regex to extract the proxies from the source.</para>
        /// <para>The regex will match proxies of the form 127.0.0.1:8888.</para>
        /// <para>This is useful for example when the server returns proxies in a HTML, XML or JSON format and not one proxy per line.</para>
        /// <para>This is also useful when the server returns proxies with unix-like line endings.</para>
        /// </summary>
        public bool ParseWithIPRegex { get { return parseWithIPRegex; } set { parseWithIPRegex = value; OnPropertyChanged(); } }
        #endregion

        #region Cloudflare
        private bool alwaysGetClearance = false;
        /// <summary>Whether to avoid storing the clearance cookie for future use when querying the site from the same IP (proxy).</summary>
        public bool AlwaysGetClearance { get { return alwaysGetClearance; } set { alwaysGetClearance = value; OnPropertyChanged(); } }
        #endregion

        #region Global Keys
        private string[] globalBanKeys = new string[] { };
        /// <summary>
        /// <para>The array of possible bad replies from a proxy server.</para>
        /// <para>The proxy will be banned when one of the keys is found.</para>
        /// <para>These are useful to immediately blacklist proxies that need a captive portal or that only accept IPs from a given region.</para>
        /// </summary>
        public string[] GlobalBanKeys { get { return globalBanKeys; } set { globalBanKeys = value; OnPropertyChanged(); } }

        private string[] globalRetryKeys = new string[] { };
        /// <summary>
        /// <para>The array of possible temporary bad replies from a proxy server.</para>
        /// <para>The proxy will be retried on the next check.</para>
        /// <para>These are useful when the proxy is temporarily too busy because of high traffic.</para>
        /// </summary>
        public string[] GlobalRetryKeys { get { return globalRetryKeys; } set { globalRetryKeys = value; OnPropertyChanged(); } }
        #endregion
    }
}
