using RuriLib.Runner;
using System;

namespace RuriLib.Models
{
    /// <summary>
    /// Contains the essential information of a Runner Session.
    /// </summary>
    public class RunnerSessionData : Persistable<Guid>
    {
        /// <summary>The name of the selected Config.</summary>
        public string Config { get; set; } = "";

        /// <summary>The name of the selected Wordlist.</summary>
        public string Wordlist { get; set; } = "";

        /// <summary>The amount of bots selected.</summary>
        public int Bots { get; set; } = 1;

        /// <summary>The proxy mode selected.</summary>
        public ProxyMode ProxyMode { get; set; } = ProxyMode.Default;
    }
}
