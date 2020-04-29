using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuriLib.ViewModels
{
    /// <summary>
    /// Provides settings for the proxy manager.
    /// </summary>
    public class ProxyManagerSettings : ViewModelBase
    {
        private ObservableCollection<string> proxySiteUrls = new ObservableCollection<string>();
        /// <summary>The list of the urls for sites to test the proxies on.</summary>
        public ObservableCollection<string> ProxySiteUrls { get { return proxySiteUrls; } set { proxySiteUrls = value; OnPropertyChanged(); } }

        private ObservableCollection<string> proxyKeys = new ObservableCollection<string>();
        /// <summary>The list of the keys for sites to test the proxies on.</summary>
        public ObservableCollection<string> ProxyKeys { get { return proxyKeys; } set { proxyKeys = value; OnPropertyChanged(); } }

        private string activeProxySiteUrl;
        /// <summary>The selected dropdown item in the proxy site urls dropdown</summary>
        public string ActiveProxySiteUrl { get { return activeProxySiteUrl; } set { activeProxySiteUrl = value; OnPropertyChanged(); } }

        private string activeProxyKey;
        /// <summary>The selected dropdown item in the proxy keys dropdown</summary>
        public string ActiveProxyKey { get { return activeProxyKey; } set { activeProxyKey = value; OnPropertyChanged(); } }
    }
}
