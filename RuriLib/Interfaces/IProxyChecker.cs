using RuriLib.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Interfaces
{
    /// <summary>
    /// Provides methods to check if proxies are working.
    /// </summary>
    public interface IProxyChecker
    {
        /// <summary>
        /// The site proxies are tested against.
        /// </summary>
        string TestSite { get; set; }

        /// <summary>
        /// The success key that can be found in the source of the test site when a proxy works correctly.
        /// </summary>
        string SuccessKey { get; set; }

        /// <summary>
        /// Whether to only check untested proxies.
        /// </summary>
        bool OnlyUntested { get; set; }

        /// <summary>
        /// The maximum proxy timeout in seconds.
        /// </summary>
        int Timeout { get; set; }

        /// <summary>
        /// The amount of parallel threads to use for the check.
        /// </summary>
        int BotsAmount { get; set; }

        /// <summary>
        /// Checks all the proxies asynchronously and reports progress and check result.
        /// </summary>
        /// <param name="proxies">The proxies to check</param>
        /// <param name="cancellationToken">The token that allows to cancel the execution</param>
        /// <param name="onResult">The action to execute when a single check is finished</param>
        /// <param name="progress">The delegate that gets called when the progress changes</param>
        /// <returns>An awaitable task.</returns>
        Task CheckAllAsync(IEnumerable<CProxy> proxies, CancellationToken cancellationToken,
            Action<CheckResult<ProxyResult>> onResult = null, IProgress<float> progress = null);

        /// <summary>
        /// Checks a proxy asynchronously.
        /// </summary>
        /// <param name="proxy">The proxy to check</param>
        /// <param name="cancellationToken">The token that allows to cancel the execution</param>
        /// <returns>The awaitable result of the check.</returns>
        Task<ProxyResult> CheckAsync(CProxy proxy, CancellationToken cancellationToken);
    }
}
