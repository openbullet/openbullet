using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuriLib.Models.Stats
{
    /// <summary>
    /// A container for all proxy-related statistics of an IRunner.
    /// </summary>
    public struct RunnerStatsProxies
    {
        /// <summary>
        /// The total amount of loaded proxies.
        /// </summary>
        public int Total { get; }

        /// <summary>
        /// The amount of proxies that are alive.
        /// </summary>
        public int Alive { get; }

        /// <summary>
        /// The amount of banned proxies.
        /// </summary>
        public int Banned { get; }

        /// <summary>
        /// The amount of bad proxies.
        /// </summary>
        public int Bad { get; }

        /// <summary>
        /// Reports proxy-related statistics.
        /// </summary>
        /// <param name="total">The total amount of checked proxies</param>
        /// <param name="alive">The amount of proxies that are alive</param>
        /// <param name="banned">The amount of banned proxies</param>
        /// <param name="bad">The amount of bad proxies</param>
        public RunnerStatsProxies(int total, int alive, int banned, int bad)
        {
            Total = total;
            Alive = alive;
            Banned = banned;
            Bad = bad;
        }
    }
}
