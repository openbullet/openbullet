using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RuriLib.Models
{
    /// <summary>
    /// Settings for customizeable data types used in the library.
    /// </summary>
    public class EnvironmentSettings
    {
        /// <summary>List of custom Wordlist Types.</summary>
        public List<WordlistType> WordlistTypes { get; set; } = new List<WordlistType>();

        /// <summary>List of custom KeyChain Types.</summary>
        public List<CustomKeychain> CustomKeychains { get; set; } = new List<CustomKeychain>();

        /// <summary>List of custom Export Formats.</summary>
        public List<ExportFormat> ExportFormats { get; set; } = new List<ExportFormat>();

        /// <summary>
        /// Gets the names of all Wordlist Types.
        /// </summary>
        /// <returns>The list of all the names of custom Wordlist Types</returns>
        public List<string> GetWordlistTypeNames()
        {
            return WordlistTypes.Select(w => w.Name).ToList();
        }

        /// <summary>
        /// Automatically recognizes a Wordlist Type between the ones available by matching the Regex patterns and returning the first successful match.
        /// </summary>
        /// <param name="data">The data for which you want to recognize the Wordlist Type</param>
        /// <returns>The correct Wordlist Type or (if every Regex match failed) the first one</returns>
        public string RecognizeWordlistType(string data)
        {
            foreach (var type in WordlistTypes)
            {
                if (Regex.Match(data, type.Regex).Success)
                {
                    return type.Name;
                }
            }

            return WordlistTypes.First().Name;
        }

        /// <summary>
        /// Gets the names of all Custom KeyChains.
        /// </summary>
        /// <returns>The list of all the names of custom KeyChain types</returns>
        public List<string> GetCustomKeychainNames()
        {
            return CustomKeychains.Select(c => c.Name).ToList();
        }

        /// <summary>
        /// Gets a Custom KeyChain given its name.
        /// </summary>
        /// <param name="name">The name of the Custom KeyChain</param>
        /// <returns>The CustomKeychain object if found, a default one if not</returns>
        public CustomKeychain GetCustomKeychain(string name)
        {
            try { return CustomKeychains.First(w => w.Name == name); }
            catch { return new CustomKeychain(); }
        }

        /// <summary>
        /// Gets a Wordlist Type given its name.
        /// </summary>
        /// <param name="name">The name of the Wordlist Type</param>
        /// <returns>The WordlistType object if found, a default one if not</returns>
        public WordlistType GetWordlistType(string name)
        {
            try { return WordlistTypes.FirstOrDefault(w => w.Name == name); }
            catch { return new WordlistType(); }
        }
    }
}