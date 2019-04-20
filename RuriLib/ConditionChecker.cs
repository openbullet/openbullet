using System;
using System.Globalization;
using System.Linq;

namespace RuriLib
{
    /// <summary>
    /// The condition on which to base the outcome of a comparison.
    /// </summary>
    public enum Condition
    {
        /// <summary>A is less than B.</summary>
        LessThan,

        /// <summary>A is greater than B.</summary>
        GreaterThan,

        /// <summary>A is equal to B.</summary>
        EqualTo,

        /// <summary>A is not equal to B.</summary>
        NotEqualTo,

        /// <summary>A contains B.</summary>
        Contains,

        /// <summary>A does not contain B.</summary>
        DoesNotContain,

        /// <summary>Whether any variable can be replaced inside the string.</summary>
        Exists,

        /// <summary>Whether no variable can be replaced inside the string.</summary>
        DoesNotExist
    }

    /// <summary>
    /// Static Class used to check if a condition is true or false.
    /// </summary>
    public static class ConditionChecker
    {
        /// <summary>
        /// Verifies if a comparison is true or false.
        /// </summary>
        /// <param name="left">The left-hand term in the comparison</param>
        /// <param name="condition">The condition of the comparison</param>
        /// <param name="right">The right-hand term in the comparison</param>
        /// <param name="data">The BotData used for variable replacement</param>
        /// <returns>Whether the comparison is true or false</returns>
        public static bool Verify(string left, Condition condition, string right, BotData data)
        {
            var style = NumberStyles.Number | NumberStyles.AllowCurrencySymbol; // Needed when comparing values with a currency symbol
            var provider = new CultureInfo("en-US");
            var L = BlockBase.ReplaceValuesRecursive(left, data); // The left-hand term can accept recursive values like <LIST[*]>
            var r = BlockBase.ReplaceValues(right, data); // The right-hand term cannot

            switch (condition)
            {
                case Condition.EqualTo:
                    return L.Any(l => l == r);

                case Condition.NotEqualTo:
                    return L.Any(l => l != r);

                case Condition.GreaterThan:
                    return L.Any(l => decimal.Parse(l.Replace(',', '.'), style, provider) > decimal.Parse(r, style, provider));

                case Condition.LessThan:
                    return L.Any(l => decimal.Parse(l.Replace(',', '.'), style, provider) < decimal.Parse(r, style, provider));

                case Condition.Contains:
                    return L.Any(l => l.Contains(r));

                case Condition.DoesNotContain:
                    return L.Any(l => !l.Contains(r));

                case Condition.Exists:
                    return L.Any(l => l != left); // Returns true if any replacement took place

                case Condition.DoesNotExist:
                    return L.All(l => l == left); // Returns true if no replacement took place

                default:
                    return false;
            }
        }
    }
}
