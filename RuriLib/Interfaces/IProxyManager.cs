using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RuriLib.Models;
using RuriLib.Models.Stats;

namespace RuriLib.Interfaces
{
    /// <summary>
    /// Interface for a class that manages a pool of proxies.
    /// </summary>
    public interface IProxyManager
    {
        /// <summary>
        /// Checks all the proxies asynchronously and returns statistics.
        /// </summary>
        /// <param name="cancellationToken">The token that allows to abort execution</param>
        /// <param name="progress">The delegate that gets called when the progress changes</param>
        void CheckAll(CancellationToken cancellationToken, IProgress<float> progress = null);

        /// <summary>
        /// Whether the proxy checker is already busy.
        /// </summary>
        bool IsBusy { get; }

        /// <summary>
        /// Adds a proxy to the pool.
        /// </summary>
        /// <param name="proxy">The proxy to add</param>
        void Add(CProxy proxy);

        /// <summary>
        /// Adds multiple proxies to the pool.
        /// </summary>
        /// <param name="proxies">The collection of proxies to add</param>
        void AddRange(IEnumerable<CProxy> proxies);

        /// <summary>
        /// The pool of proxies.
        /// </summary>
        IEnumerable<CProxy> Pool { get; }

        /// <summary>
        /// Deletes a proxy from the pool.
        /// </summary>
        /// <param name="proxy">The proxy to delete</param>
        void Delete(CProxy proxy);

        /// <summary>
        /// Deletes all proxies from the pool.
        /// </summary>
        void DeleteAll();

        /// <summary>
        /// Deletes not working proxies from the pool.
        /// </summary>
        void DeleteNotWorking();

        /// <summary>
        /// Deletes duplicate proxies from the pool.
        /// </summary>
        void DeleteDuplicates();

        /// <summary>
        /// Deletes all untested proxies from the pool.
        /// </summary>
        void DeleteUntested();

        /// <summary>
        /// Retrieves the proxy manager's statistics.
        /// </summary>
        ProxyManagerStats Stats { get; }

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
        int Bots { get; set; }
    }
}
