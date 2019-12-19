using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RuriLib.ViewModels;

namespace RuriLib.Interfaces
{
    /// <summary>
    /// An interface for global settings.
    /// </summary>
    public interface IGlobals
    {
        /// <summary>
        /// The current Environment settings.
        /// </summary>
        Environment Environment { get; }

        /// <summary>
        /// The current RuriLib settings.
        /// </summary>
        RLSettingsViewModel RLSettings { get; }
    }
}
