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
        public RunnerStatsData data;

        /// <summary>
        /// The proxy-related stats.
        /// </summary>
        public RunnerStatsProxies proxies;

        /// <summary>
        /// The amount of checks per minute.
        /// </summary>
        public double cpm;

        /// <summary>
        /// The remaining captcha credit.
        /// </summary>
        public decimal credit;

        /// <summary>
        /// Reports Runner statistics.
        /// </summary>
        /// <param name="data">The data-related stats</param>
        /// <param name="proxies">The proxy-related stats</param>
        /// <param name="cpm">The amount of checks per minute</param>
        /// <param name="credit">The remaining captcha credit</param>
        public RunnerStats(RunnerStatsData data, RunnerStatsProxies proxies, double cpm, decimal credit)
        {
            this.data = data;
            this.proxies = proxies;
            this.cpm = cpm;
            this.credit = credit;
        }
    }
}
