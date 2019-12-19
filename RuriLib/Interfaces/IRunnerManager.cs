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
        /// Clears the list of runners.
        /// </summary>
        void Clear();

        /// <summary>
        /// The list of managed runners.
        /// </summary>
        IEnumerable<IRunner> Runners { get; }
    }
}
