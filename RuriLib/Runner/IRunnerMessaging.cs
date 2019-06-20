using RuriLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuriLib.Runner
{
    /// <summary>
    /// Interface used to communicate between the RunnerViewModel and the corresponding View.
    /// </summary>
    public interface IRunnerMessaging
    {
        /// <summary>A message has arrived.</summary>
        event Action<IRunnerMessaging, LogLevel, string, bool, int> MessageArrived;

        /// <summary>The status of the Master Worker changed.</summary>
        event Action<IRunnerMessaging> WorkerStatusChanged;

        /// <summary>A Hit was found.</summary>
        event Action<IRunnerMessaging, Hit> FoundHit;

        /// <summary>The proxies need to be reloaded.</summary>
        event Action<IRunnerMessaging> ReloadProxies;

        /// <summary>An Action needs to be executed on the main thread.</summary>
        event Action<IRunnerMessaging, Action> DispatchAction;

        /// <summary>The progress needs to be saved.</summary>
        event Action<IRunnerMessaging> SaveProgress;

        /// <summary>The user needs to type the custom inputs.</summary>
        event Action<IRunnerMessaging> AskCustomInputs;
    }
}
