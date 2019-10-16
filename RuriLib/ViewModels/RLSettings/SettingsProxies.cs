using Extreme.Net;
using RuriLib.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

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
        /// <summary>A remote API or website.</summary>
        Remote
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

        private int banLoopEvasion = 100;
        /// <summary>The maximum amount of times a data line ends up with a BAN status before it's marked as ToCheck.</summary>
        public int BanLoopEvasion { get { return banLoopEvasion; } set { banLoopEvasion = value; OnPropertyChanged(); } }

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
        /// <summary>The file path on disk.</summary>
        public string ReloadPath { get { return reloadPath; } set { reloadPath = value; OnPropertyChanged(); } }

        private ProxyType reloadType = ProxyType.Http;
        /// <summary>The Type of the proxies to load.</summary>
        public ProxyType ReloadType { get { return reloadType; } set { reloadType = value; OnPropertyChanged(); } }

        private int reloadInterval = 0;
        /// <summary>The amount of time between reloads in minutes (0 to only reload when all proxies are banned).</summary>
        public int ReloadInterval { get { return reloadInterval; } set { reloadInterval = value; OnPropertyChanged(); } }

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

        #region Remote Proxy Sources
        /// <summary>The sources where proxies are downloaded and parsed from.</summary>
        public ObservableCollection<RemoteProxySource> RemoteProxySources { get; set; } = new ObservableCollection<RemoteProxySource>();

        /// <summary>
        /// Removes a RemoteProxySource given its id.
        /// </summary>
        /// <param name="id">The id of the RemoteProxySource</param>
        public void RemoveRemoteProxySourceById(int id)
        {
            RemoteProxySources.Remove(GetRemoteProxySourceById(id));
        }

        /// <summary>
        /// Gets a RemoteProxySource given its id.
        /// </summary>
        /// <param name="id">The id of the RemoteProxySource</param>
        /// <returns>The wanted RemoteProxySource. Null if not found.</returns>
        public RemoteProxySource GetRemoteProxySourceById(int id)
        {
            return RemoteProxySources.FirstOrDefault(s => s.Id == id);
        }
        #endregion

        /// <summary>
        /// Resets the properties to their default value.
        /// </summary>
        public void Reset()
        {
            SettingsProxies def = new SettingsProxies();
            IList<PropertyInfo> props = new List<PropertyInfo>(typeof(SettingsProxies).GetProperties());

            foreach (PropertyInfo prop in props)
            {
                prop.SetValue(this, prop.GetValue(def, null));
            }
        }
    }
}
