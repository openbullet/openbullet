using System.Collections.Generic;

namespace RuriLib.Models
{
    /// <summary>
    /// Class that represents a type of Wordlist with rules to check the validity and to slice the data given a separator.
    /// </summary>
    public class WordlistType
    {
        /// <summary>Whether to check if the regex successfully matches the input data.</summary>
        public bool Verify { get; set; } = false;

        /// <summary>The regular expression that validates the input data.</summary>
        public string Regex { get; set; } = "^.*$";

        /// <summary>The name of the Wordlist Type.</summary>
        public string Name { get; set; } = "Default";

        /// <summary>The separator used for slicing the input data into a list of strings.</summary>
        public string Separator { get; set; } = ":";

        /// <summary>
        /// <para>The list of variable names of the slices that will be created from the input data.</para>
        /// <para>Each slice should be used to generate a pair of values with a variable name and a value taken from the split input data.</para>
        /// </summary>
        public List<string> Slices { get; set; } = new List<string>();
    }
}
