using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RuriLib.ViewModels;

namespace RuriLib.Interfaces
{
    /// <summary>
    /// Interface for a class that manages a collection of configs.
    /// </summary>
    public interface IConfigManager
    {
        /// <summary>
        /// Adds a new config.
        /// </summary>
        /// <param name="config">The config to add</param>
        void Add(ConfigViewModel config);

        /// <summary>
        /// Rescans local folders for an updated list of configs.
        /// </summary>
        void Rescan();

        /// <summary>
        /// The collection of available configs.
        /// </summary>
        IEnumerable<ConfigViewModel> Configs { get; }

        /// <summary>
        /// Removes a config.
        /// </summary>
        /// <param name="config">The config to remove</param>
        void Remove(ConfigViewModel config);

        /// <summary>
        /// Removes multiple configs.
        /// </summary>
        /// <param name="configs">The configs to remove</param>
        void Remove(IEnumerable<ConfigViewModel> configs);

        /// <summary>
        /// Updates a config.
        /// </summary>
        /// <param name="config">The config to update</param>
        void Update(ConfigViewModel config);
    }
}
