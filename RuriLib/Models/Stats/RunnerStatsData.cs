using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuriLib.Models.Stats
{
    /// <summary>
    /// A container for all data-related statistics of an IRunner.
    /// </summary>
    public struct RunnerStatsData
    {
        /// <summary>
        /// The total amount of checked data.
        /// </summary>
        public int Total { get; }

        /// <summary>
        /// The amount of hits.
        /// </summary>
        public int Hits { get; }

        /// <summary>
        /// The amount of custom results.
        /// </summary>
        public int Custom { get; }

        /// <summary>
        /// The amount of bad results.
        /// </summary>
        public int Bad { get; }

        /// <summary>
        /// The amount of retries due to a RETRY or BAN status.
        /// </summary>
        public int Retries { get; }

        /// <summary>
        /// The amount of results that need to be furtherly checked.
        /// </summary>
        public int ToCheck { get; }

        /// <summary>
        /// Reports data-related statistics.
        /// </summary>
        /// <param name="total">The total amount of checked data</param>
        /// <param name="hits">The amount of hits</param>
        /// <param name="custom">The amount of custom results</param>
        /// <param name="bad">The amount of bad results</param>
        /// <param name="retries">The amount of retries due to a RETRY or BAN status</param>
        /// <param name="toCheck">The amount of results that need to be furtherly checked</param>
        public RunnerStatsData(int total, int hits, int custom, int bad, int retries, int toCheck)
        {
            Total = total;
            Hits = hits;
            Custom = custom;
            Bad = bad;
            Retries = retries;
            ToCheck = toCheck;
        }
    }
}
