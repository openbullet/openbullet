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
        public int total;

        /// <summary>
        /// The amount of hits.
        /// </summary>
        public int hits;

        /// <summary>
        /// The amount of custom results.
        /// </summary>
        public int custom;

        /// <summary>
        /// The amount of bad results.
        /// </summary>
        public int bad;

        /// <summary>
        /// The amount of retries due to a RETRY or BAN status.
        /// </summary>
        public int retries;

        /// <summary>
        /// The amount of results that need to be furtherly checked.
        /// </summary>
        public int toCheck;

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
            this.total = total;
            this.hits = hits;
            this.custom = custom;
            this.bad = bad;
            this.retries = retries;
            this.toCheck = toCheck;
        }
    }
}
