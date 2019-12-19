using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuriLib.Models.Stats
{
    /// <summary>
    /// A container for all statistics of an IRunner progress.
    /// </summary>
    public struct RunnerStats
    {
        /// <summary>
        /// The data-related stats.
        /// </summary>
        public RunnerStatsData Data { get; }

        /// <summary>
        /// The proxy-related stats.
        /// </summary>
        public RunnerStatsProxies Proxies { get; }

        /// <summary>
        /// The amount of checks per minute.
        /// </summary>
        public double CPM { get; }

        /// <summary>
        /// The remaining captcha credit.
        /// </summary>
        public decimal Credit { get; }
    }
}
