using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuriLib.Models.Stats
{
    /// <summary>
    /// A container for all statistics of an IProxyManager.
    /// </summary>
    public struct ProxyManagerStats
    {
        /// <summary>
        /// The total amount of proxies in the pool.
        /// </summary>
        public int Total { get; }

        /// <summary>
        /// The amount of tested proxies.
        /// </summary>
        public int Tested { get; }

        /// <summary>
        /// The amount of working proxies.
        /// </summary>
        public int Working { get; }

        /// <summary>
        /// The amount of HTTP proxies in the pool.
        /// </summary>
        public int Http { get; }

        /// <summary>
        /// The amount of SOCKS4 proxies in the pool.
        /// </summary>
        public int Socks4 { get; }

        /// <summary>
        /// The amount of SOCKS4A proxies in the pool.
        /// </summary>
        public int Socks4a { get; }

        /// <summary>
        /// The amount of SOCKS5 proxies in the pool.
        /// </summary>
        public int Socks5 { get; }
    }
}
