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
        /// Adds a config to the collection.
        /// </summary>
        /// <param name="config">The config to add</param>
        void Add(ConfigViewModel config);

        /// <summary>
        /// Saves the currently selected config.
        /// </summary>
        void Save();

        /// <summary>
        /// Rescans local folders for an updated list of configs.
        /// </summary>
        void Rescan();

        /// <summary>
        /// The collection of available configs.
        /// </summary>
        IEnumerable<ConfigViewModel> Configs { get; }

        /// <summary>
        /// The currently selected config.
        /// </summary>
        ConfigViewModel CurrentConfig { get; set; }

        /// <summary>
        /// Deletes a config from both the collection and the disk.
        /// </summary>
        /// <param name="config">The config to delete</param>
        void Delete(ConfigViewModel config);
    }
}
