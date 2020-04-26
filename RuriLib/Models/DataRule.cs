using Newtonsoft.Json;
using RuriLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuriLib.Models
{
    /// <summary>
    /// The condition for which a Rule is tested.
    /// </summary>
    public enum RuleType
    {
        /// <summary>The slice must contain the given characters.</summary>
        MustContain,

        /// <summary>The slice must not contain the given characters.</summary>
        MustNotContain,

        /// <summary>The slice's length must be greater or equal to a given number.</summary>
        MinLength,

        /// <summary>The slice's length must be smaller or equal to a given number.</summary>
        MaxLength,

        /// <summary>The slice must match a given regex pattern.</summary>
        MustMatchRegex
    }

    /// <summary>
    /// Represents a rule that the data line must respect in order to be valid for a given Config.
    /// </summary>
    public class DataRule : ViewModelBase
    {
        /// <summary>
        /// List of Rule types that are displayed in the combobox.
        /// </summary>
        [JsonIgnore]
        public List<RuleType> RuleTypes { get; set; } = new List<RuleType>(Enum.GetValues(typeof(RuleType)).Cast<RuleType>());

        /// <summary>
        /// List of Rule strings that are displayed in the combobox.
        /// </summary>
        [JsonIgnore]
        public List<string> RuleStrings { get; set; } = new List<string>(new[] { "Lowercase", "Uppercase", "Digit", "Symbol" });

        private string sliceName = "";
        /// <summary>The name of the specific slice (defined in the WordlistType) this rule refers to.</summary>
        public string SliceName { get { return sliceName; } set { sliceName = value; OnPropertyChanged(); } }

        private RuleType ruleType = RuleType.MustContain;
        /// <summary>The type of the rule.</summary>
        public RuleType RuleType { get { return ruleType; } set { ruleType = value; OnPropertyChanged(); } }

        private string ruleString = "Lowercase";
        /// <summary>The characters to search in the sliced value. Defaults are Lowercase, Uppercase, Digit, Symbol. Custom character sets can be created by concatenating the characters in a single string (e.g. ABCDEF).</summary>
        public string RuleString { get { return ruleString; } set { ruleString = value; OnPropertyChanged(); } }

        /// <summary>The id of the rule.</summary>
        public int Id { get; set; }

        /// <summary>Whether the type has been initialized. Used in the View.</summary>
        [JsonIgnore]
        public bool TypeInitialized { get; set; } = false;

        /// <summary>Whether the right-hand term has been initialized. Used in the View.</summary>
        [JsonIgnore]
        public bool StringInitialized { get; set; } = false;

        /// <summary>
        /// Creates a DataRule given an id.
        /// </summary>
        /// <param name="id">The unique id of the rule</param>
        public DataRule(int id)
        {
            Id = id;
        }
    }
}
