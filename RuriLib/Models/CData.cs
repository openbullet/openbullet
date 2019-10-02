using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RuriLib.Models
{
    /// <summary>
    /// Represents a data line that needs to be checked.
    /// </summary>
    public class CData
    {
        /// <summary>The actual content of the line.</summary>
        public string Data { get; set; }

        /// <summary>The WordlistType of the Wordlist the line belongs to.</summary>
        public WordlistType Type { get; set; }

        /// <summary>The amount of times the data has been retried with a different proxy.</summary>
        public int Retries { get; set; } = 0;

        /// <summary>Whether the data line respects the regex verification.</summary>
        public bool IsValid
        {
            get
            {
                if (Type.Verify) return Regex.Match(Data, Type.Regex).Success;
                else return true;
            }
        }

        /// <summary>
        /// Creates a CData object given a data line and the WordlistType.
        /// </summary>
        /// <param name="data">The data line</param>
        /// <param name="type">The WordlistType of the Wordlist the data line belongs to</param>
        public CData(string data, WordlistType type)
        {
            Data = data;
            Type = type;
        }

        /// <summary>
        /// Gets all the variables that need to be set after slicing the data line.
        /// </summary>
        /// <param name="encode">Whether the returned variable values should be URLencoded</param>
        /// <returns>The variables that need to be set inside the Bot's VariableList</returns>
        public List<CVar> GetVariables(bool encode)
        {
            return Data
                .Split(new string[] { Type.Separator }, StringSplitOptions.None)
                .Zip(Type.Slices, (k, v) => new { k, v })
                .Select(x => new CVar(x.v, encode ? Uri.EscapeDataString(x.k) : x.k, false, true))
                .ToList();
        }

        /// <summary>
        /// Checks if the data line respects all the data rules present in a Config.
        /// </summary>
        /// <param name="rules">The list of rules of the Config</param>
        /// <returns>Whether the data line respects the rules</returns>
        public bool RespectsRules(List<DataRule> rules)
        {
            var valid = true;
            var variables = GetVariables(false);
            foreach(var rule in rules)
            {
                try
                {
                    var slice = (string)variables.FirstOrDefault(v => v.Name == rule.SliceName).Value;

                    switch (rule.RuleType)
                    {
                        case RuleType.MinLength:
                            valid = slice.Length >= int.Parse(rule.RuleString);
                            break;

                        case RuleType.MaxLength:
                            valid = slice.Length <= int.Parse(rule.RuleString);
                            break;

                        case RuleType.MustContain:
                            valid = CheckContains(slice, rule.RuleString);
                            break;

                        case RuleType.MustNotContain:
                            valid = !CheckContains(slice, rule.RuleString);
                            break;

                        case RuleType.MustMatchRegex:
                            valid = Regex.Match(slice, rule.RuleString).Success;
                            break;
                    }

                    if (!valid) return false;
                }
                catch { }
            }

            return true;
        }

        /// <summary>
        /// Checks if a data line contains certain characters.
        /// </summary>
        /// <param name="input">The data line</param>
        /// <param name="what">The characters to search. Defaults are Lowercase, Uppercase, Digit, Symbol. Custom character sets can be created by concatenating the characters in a single string (e.g. ABCDEF)</param>
        /// <returns></returns>
        private static bool CheckContains(string input, string what)
        {
            switch (what)
            {
                case "Lowercase":
                    return input.Any(c => char.IsLower(c));

                case "Uppercase":
                    return input.Any(c => char.IsUpper(c));

                case "Digit":
                    return input.Any(c => char.IsDigit(c));

                case "Symbol":
                    return input.Any(c => char.IsSymbol(c) || char.IsPunctuation(c));

                default:
                    foreach(var c in what.ToCharArray())
                    {
                        if (input.Contains(c)) return true;
                    }
                    return false;
            }
        }
    }
}
