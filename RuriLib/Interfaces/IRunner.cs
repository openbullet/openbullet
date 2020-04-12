using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RuriLib.Models;
using RuriLib.Models.Stats;
using RuriLib.Runner;

namespace RuriLib.Interfaces
{
    /// <summary>
    /// Interface for a generic Runner.
    /// </summary>
    public interface IRunner
    {
        /// <summary>
        /// Starts the Runner.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the Runner.
        /// </summary>
        void Stop();

        /// <summary>
        /// Whether the Runner is already busy.
        /// </summary>
        bool Busy { get; }

        /// <summary>
        /// The checking progress percentage (0 to 100).
        /// </summary>
        int Progress { get; }

        /// <summary>
        /// The collection of data that was checked with a positive outcome.
        /// </summary>
        IEnumerable<ValidData> Checked { get; }

        /// <summary>
        /// Progress and statistics.
        /// </summary>
        RunnerStats Stats { get; }

        /// <summary>
        /// The currently selected Config.
        /// </summary>
        Config Config { get; }

        /// <summary>
        /// Sets a Config in the IRunner.
        /// </summary>
        /// <param name="config">The Config to set</param>
        /// <param name="setRecommended">Whether to automatically change the BotsAmount to the suggested value</param>
        void SetConfig(Config config, bool setRecommended);

        /// <summary>
        /// The currently selected Wordlist.
        /// </summary>
        Wordlist Wordlist { get; }

        /// <summary>
        /// Sets a Wordlist in the IRunner.
        /// </summary>
        /// <param name="wordlist">The Wordlist to set</param>
        void SetWordlist(Wordlist wordlist);

        /// <summary>
        /// The amount of concurrent Bots.
        /// </summary>
        int BotsAmount { get; set; }

        /// <summary>
        /// Whether to use proxies during the checking process.
        /// </summary>
        ProxyMode ProxyMode { get; set; }

        /// <summary>
        /// The starting point of the Wordlist.
        /// </summary>
        int StartingPoint { get; set; }
    }
}
