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

        /// <summary>Fired when a new message needs to be logged.</summary>
        event Action<IRunnerMessaging, LogLevel, string, bool, int> MessageArrived;

        /// <summary>Fired when the Master Worker status changed.</summary>
        event Action<IRunnerMessaging> WorkerStatusChanged;

        /// <summary>Fired when a Hit was found.</summary>
        event Action<IRunnerMessaging, Hit> FoundHit;

        /// <summary>Fired when proxies need to be reloaded.</summary>
        event Action<IRunnerMessaging> ReloadProxies;

        /// <summary>/// Fired when an Action could change the UI and needs to be dispatched to another thread (usually it's handled by the UI thread).</summary>
        event Action<IRunnerMessaging, Action> DispatchAction;

        /// <summary>Fired when the progress record needs to be saved to the Database.</summary>
        event Action<IRunnerMessaging> SaveProgress;

        /// <summary>Fired when custom inputs from the user are required.</summary>
        event Action<IRunnerMessaging> AskCustomInputs;

        /// <summary>Fired when the currently selected Config changed.</summary>
        event Action<IRunnerMessaging> ConfigChanged;

        /// <summary>Fired when the currently selected Wordlist changed.</summary>
        event Action<IRunnerMessaging> WordlistChanged;
    }
}
