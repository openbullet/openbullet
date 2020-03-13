using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuriLib.Interfaces
{
    /// <summary>
    /// Interface for an application that depends on RuriLib.
    /// </summary>
    public interface IApplication
    {
        /// <summary>
        /// The Runner Manager.
        /// </summary>
        IRunnerManager RunnerManager { get; }

        /// <summary>
        /// The Proxy Manager.
        /// </summary>
        IProxyManager ProxyManager { get; }

        /// <summary>
        /// The Proxy Checker.
        /// </summary>
        IProxyChecker ProxyChecker { get; }

        /// <summary>
        /// The Wordlist Manager.
        /// </summary>
        IWordlistManager WordlistManager { get; }

        /// <summary>
        /// The Config Manager.
        /// </summary>
        IConfigManager ConfigManager { get; }

        /// <summary>
        /// The Hits Database.
        /// </summary>
        IHitsDB HitsDB { get; }

        /// <summary>
        /// The user interaction provider.
        /// </summary>
        IAlerter Alerter { get; }

        /// <summary>
        /// The logging system.
        /// </summary>
        ILogger Logger { get; }

        /// <summary>
        /// The global settings.
        /// </summary>
        ISettings Settings { get; }
    }
}
