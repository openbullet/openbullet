using System.Collections.Generic;
using RuriLib.Models;
using RuriLib.Models.Stats;

namespace RuriLib.Interfaces
{
    /// <summary>
    /// Interface for a class that manages a collection of proxies.
    /// </summary>
    public interface IProxyManager
    {
        /// <summary>
        /// Adds a proxy to the collection.
        /// </summary>
        /// <param name="proxy">The proxy to add</param>
        void Add(CProxy proxy);

        /// <summary>
        /// Adds multiple proxies to the collection.
        /// </summary>
        /// <param name="proxies">The collection of proxies to add</param>
        void AddRange(IEnumerable<CProxy> proxies);

        /// <summary>
        /// The collection of proxies.
        /// </summary>
        IEnumerable<CProxy> Proxies { get; }

        /// <summary>
        /// Removes a proxy from the collection.
        /// </summary>
        /// <param name="proxy">The proxy to remove</param>
        void Remove(CProxy proxy);

        /// <summary>
        /// Removes multiple proxies from the collection.
        /// </summary>
        /// <param name="proxies">The proxies to remove</param>
        void Remove(IEnumerable<CProxy> proxies);

        /// <summary>
        /// Updates a proxy in the collection.
        /// </summary>
        /// <param name="proxy">The proxy to update</param>
        void Update(CProxy proxy);

        /// <summary>
        /// Removes all proxies from the collection.
        /// </summary>
        void RemoveAll();

        /// <summary>
        /// Removes not working proxies from the collection.
        /// </summary>
        void RemoveNotWorking();

        /// <summary>
        /// Removes duplicate proxies from the collection.
        /// </summary>
        void RemoveDuplicates();

        /// <summary>
        /// Removes all untested proxies from the collection.
        /// </summary>
        void RemoveUntested();

        /// <summary>
        /// Retrieves the proxy manager's statistics.
        /// </summary>
        ProxyManagerStats Stats { get; }
    }
}
