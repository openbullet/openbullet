using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuriLib.Interfaces
{
    /// <summary>
    /// Interface for requesting user interaction (e.g. confirmations).
    /// </summary>
    public interface IAlerter
    {
        /// <summary>
        /// Asks a yes or no question to the user.
        /// </summary>
        /// <param name="message">The body of the alert</param>
        /// <param name="title">The caption of the alert</param>
        /// <returns>True if the user answered yes.</returns>
        bool YesOrNo(string message, string title);
    }
}
