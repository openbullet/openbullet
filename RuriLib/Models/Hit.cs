using LiteDB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RuriLib.Models
{
    /// <summary>
    /// Represents the outcome of a successful check, used as Data Type in a Database.
    /// </summary>
    public class Hit : Persistable<Guid>
    {
        /// <summary>The data line that was used.</summary>
        public string Data { get; set; }

        /// <summary>The list of all variables marked as Capture.</summary>
        public VariableList CapturedData { get; set; }

        /// <summary>The list of all variables marked as Capture as a chained string.</summary>
        [JsonIgnore]
        [BsonIgnore]
        public string CapturedString => CapturedData.ToCaptureString();

        /// <summary>The proxy that was used.</summary>
        public string Proxy { get; set; }

        /// <summary>The timestamp of when the Hit was found.</summary>
        public DateTime Date { get; set; }

        /// <summary>The type of Hit.</summary>
        public string Type { get; set; }

        /// <summary>The name of the Config that was used.</summary>
        public string ConfigName { get; set; }

        /// <summary>The name of the Wordlist that was used.</summary>
        public string WordlistName { get; set; }

        /// <summary>Needed for NoSQL deserialization.</summary>
        public Hit() { }

        /// <summary>
        /// Creates a Hit given its details.
        /// </summary>
        /// <param name="data">The data line used</param>
        /// <param name="capturedData">The VariableList of all captured data</param>
        /// <param name="proxy">The proxy used</param>
        /// <param name="type">The type of the Hit</param>
        /// <param name="configName">The Config's name</param>
        /// <param name="wordlistName">The Wordlist's name</param>
        public Hit(string data, VariableList capturedData, string proxy, string type, string configName, string wordlistName)
        {
            Data = data;
            CapturedData = capturedData;
            Proxy = proxy;
            Date = DateTime.Now;
            Type = type;
            ConfigName = configName;
            WordlistName = wordlistName;
        }

        /// <summary>
        /// Outputs the Hit details in a given format.
        /// </summary>
        /// <param name="format">The format in which variables should be replaced</param>
        /// <returns>The formatted string with all variables replaced</returns>
        public string ToFormattedString(string format)
        {
            StringBuilder sb = new StringBuilder(format)
                .Replace("<DATA>", Data)
                .Replace("<PROXY>", Proxy)
                .Replace("<DATE>", Date.ToLongDateString() + " " + Date.ToLongTimeString())
                .Replace("<CONFIG>", ConfigName)
                .Replace("<WORDLIST>", WordlistName)
                .Replace("<TYPE>", Type)
                .Replace("<CAPTURE>", CapturedData.ToCaptureString());

            foreach(var cap in CapturedData.All)
                sb.Replace("<" + cap.Name + ">", cap.ToString());   

            return sb.ToString();
        }

        /// <summary>
        /// Gets a unique hash of the hit.
        /// </summary>
        /// <param name="ignoreWordlistName">Whether the wordlist name should affect the generated hash</param>
        /// <returns>The hash code</returns>
        public int GetHashCode(bool ignoreWordlistName = true)
        {
            var id = ignoreWordlistName ? Data + ConfigName : Data + ConfigName + WordlistName;
            return id.GetHashCode();
        }
    }
}
