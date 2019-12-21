using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuriLib.Interfaces
{
    /// <summary>
    /// Interface for a logging system.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs a message.
        /// </summary>
        /// <param name="message">The message body</param>
        /// <param name="level">The message level of criticality</param>
        /// <param name="prompt">Whether to prompt the entry to the user in an immediately visible way</param>
        /// <param name="timeout">The amount of seconds after which the prompt will close (0 = never close)</param>
        void Log(string message, LogLevel level = LogLevel.Info, bool prompt = false, int timeout = 0);

        /// <summary>
        /// All entries in the logging system.
        /// </summary>
        IEnumerable<LogEntry> Entries { get; }

        /// <summary>
        /// Whether logging is enabled.
        /// </summary>
        bool Enabled { get; }

        /// <summary>
        /// The maximum amount of entries that can be stored (0 = unlimited).
        /// </summary>
        int BufferSize { get; }
    }
}
