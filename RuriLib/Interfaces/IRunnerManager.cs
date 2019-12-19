using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RuriLib.Runner;

namespace RuriLib.Interfaces
{
    /// <summary>
    /// Interface for a class that manages multiple runners.
    /// </summary>
    public interface IRunnerManager
    {
        /// <summary>
        /// Starts all the runner.
        /// </summary>
        void StartAll();

        /// <summary>
        /// Stops all the running runners.
        /// </summary>
        void StopAll();

        /// <summary>
        /// The list of managed runners.
        /// </summary>
        IEnumerable<IRunner> Runners { get; }

        /// <summary>
        /// Adds a new runner to the manager.
        /// </summary>
        /// <param name="runner">The runner to add</param>
        void Add(IRunner runner);

        /// <summary>
        /// Creates a new runner and adds it to the manager.
        /// </summary>
        /// <returns>The newly created runner</returns>
        IRunner Create();

        /// <summary>
        /// Removes a runner from the manager.
        /// </summary>
        /// <param name="runner">The runner to remove</param>
        void Remove(IRunner runner);

        /// <summary>
        /// Clears the list of runners.
        /// </summary>
        void Clear();
    }
}
