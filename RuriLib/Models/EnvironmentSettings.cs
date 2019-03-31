using System.Collections.Generic;
using System.Linq;

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