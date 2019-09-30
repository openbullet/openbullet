using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuriLib.Models
{
    /// <summary>
    /// The class that gets JSON-encoded and gets sent to the Webhook URL when a hit is found (if enabled).
    /// </summary>
    public class WebhookFormat
    {
        /// <summary>
        /// Initializes the WebhookFormat object.
        /// </summary>
        /// <param name="data">The input data</param>
        /// <param name="type">The hit type</param>
        /// <param name="capturedData">The string-encoded captured data</param>
        /// <param name="timestamp">The unix timestamp</param>
        /// <param name="configName">The config name</param>
        /// <param name="configAuthor">The config author</param>
        /// <param name="user">The user that is calling the webhook</param>
        public WebhookFormat(string data, string type, string capturedData, DateTime timestamp, string configName, string configAuthor, string user)
        {
            Data = data;
            Type = type;
            CapturedData = capturedData;
            Timestamp = Math.Round((timestamp.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
            ConfigName = configName;
            ConfigAuthor = configAuthor;
            User = user;
        }

        /// <summary>The input data.</summary>
        public string Data { get; set; }

        /// <summary>The hit type.</summary>
        public string Type { get; set; }

        /// <summary>The string-encoded captured data.</summary>
        public string CapturedData { get; set; }

        /// <summary>The unix timestamp.</summary>
        public double Timestamp { get; set; }

        /// <summary>The config name.</summary>
        public string ConfigName { get; set; }

        /// <summary>The config author.</summary>
        public string ConfigAuthor { get; set; }

        /// <summary>The user that is calling the webhook.</summary>
        public string User { get; set; }
    }
}
