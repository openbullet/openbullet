using System;

namespace RuriLib.Models
{
    /// <summary>
    /// A record that, given a Config's name and a Wordlist's location, returns how many data lines have already been checked.
    /// </summary>
    public class Record
    {
        /// <summary>Needed for NoSQL storage.</summary>
        public Guid Id { get; set; }

        /// <summary>The name of the Config.</summary>
        public string ConfigName { get; set; }

        /// <summary>The location on disk of the Wordlist.</summary>
        public string WordlistLocation { get; set; }

        /// <summary>How many data lines were already checked.</summary>
        public int Checkpoint { get; set; }

        /// <summary>Needed for NoSQL deserialization.</summary>
        public Record() { }

        /// <summary>
        /// Creates a Record to be stored in the Database.
        /// </summary>
        /// <param name="configName">The name of the Config used in the check</param>
        /// <param name="wordlistLocation">The location on disk of the Wordlist used in the check</param>
        /// <param name="checkpoint">The amount of data lines checked when creating the record</param>
        public Record(string configName, string wordlistLocation, int checkpoint)
        {
            ConfigName = configName;
            WordlistLocation = wordlistLocation;
            Checkpoint = checkpoint;
        }
    }
}
