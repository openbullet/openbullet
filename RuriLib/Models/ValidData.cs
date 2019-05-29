using Extreme.Net;
using RuriLib.ViewModels;
using System;
using System.Collections.Generic;

namespace RuriLib.Models
{
    /// <summary>
    /// Represents the outcome of a check which can be represented in a list and can give information on the details of the check.
    /// </summary>
    public class ValidData : ViewModelBase
    {
        private string data;
        /// <summary>The data line that was checked.</summary>
        public string Data { get { return data; } set { data = value; OnPropertyChanged(); } }

        private string proxy;
        /// <summary>The proxy that was used for the check.</summary>
        public string Proxy { get { return proxy; } set { proxy = value; OnPropertyChanged(); } }

        private ProxyType proxyType;
        /// <summary>The proxy type.</summary>
        public ProxyType ProxyType { get { return proxyType; } set { proxyType = value; OnPropertyChanged(); } }

        private BotStatus result;
        /// <summary>The result of the check.</summary>
        public BotStatus Result { get { return result; } set { result = value; OnPropertyChanged(); } }

        private string type;
        /// <summary>The type of the result.</summary>
        public string Type { get { return type; } set { type = value; OnPropertyChanged(); } }

        private string capturedData;
        /// <summary>The data captured during the check.</summary>
        public string CapturedData { get { return capturedData; } set { capturedData = value; OnPropertyChanged(); } }

        private int unixDate;
        /// <summary>The unix timestamp of the check completion.</summary>
        public int UnixDate { get { return unixDate; } set { unixDate = value; OnPropertyChanged(); OnPropertyChanged(); } }

        /// <summary>The timestamp of the check completion as a formatted date.</summary>
        public string Timestamp { get { return (new DateTime(1970, 1, 1)).AddSeconds(UnixDate).ToShortDateString(); } }

        /// <summary>The timestamp of the check completion as a DateTime object.</summary>
        public DateTime Time { get { return (new DateTime(1970, 1, 1)).AddSeconds(UnixDate); } }

        private string source;
        /// <summary>The contents of the last page's source code at the end of the check.</summary>
        public string Source { get { return source; } set { source = value; OnPropertyChanged(); } }

        private List<LogEntry> log;
        /// <summary>The entire log with the details of the check.</summary>
        public List<LogEntry> Log { get { return log; } set { log = value; OnPropertyChanged(); } }

        /// <summary>
        /// Creates a ValidData object after a valid check.
        /// </summary>
        /// <param name="data">The data line that was used in the check</param>
        /// <param name="proxy">The proxy that was used for the check (empty string if none)</param>
        /// <param name="proxyType">The proxy type</param>
        /// <param name="result">The result of the check</param>
        /// <param name="type">The result type</param>
        /// <param name="capturedData">The data captured during the check</param>
        /// <param name="source">The last page source code of the check</param>
        /// <param name="log">The detailed log of the check</param>
        public ValidData(string data, string proxy, ProxyType proxyType, BotStatus result, string type, string capturedData, string source, List<LogEntry> log)
        {
            Data = data;
            Proxy = proxy;
            Result = result;
            Type = type;
            CapturedData = capturedData;
            UnixDate = (int)Math.Round((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds);
            Source = source;
            Log = log;
        }
    }
}
