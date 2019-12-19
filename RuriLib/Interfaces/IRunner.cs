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
        /// <param name="cancellationToken">The token that allows to abort execution</param>
        /// <param name="progress">The delegate that gets called when the progress changes</param>
        void Start(CancellationToken cancellationToken, IProgress<float> progress = null);

        /// <summary>
        /// Whether the Runner is already busy.
        /// </summary>
        bool IsBusy { get; }

        /// <summary>
        /// The list of found hits.
        /// </summary>
        IEnumerable<Hit> Hits { get; }

        /// <summary>
        /// Progress and statistics.
        /// </summary>
        RunnerStats Stats { get; }

        /// <summary>
        /// The currently selected Config.
        /// </summary>
        Config Config { get; set; }

        /// <summary>
        /// The currently selected Wordlist.
        /// </summary>
        Wordlist Wordlist { get; set; }

        /// <summary>
        /// The amount of concurrent Bots.
        /// </summary>
        int Bots { get; set; }

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
