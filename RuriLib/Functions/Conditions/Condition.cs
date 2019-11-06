using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace RuriLib.Functions.Conditions
{
    /// <summary>
    /// The condition on which to base the outcome of a comparison.
    /// </summary>
    public enum Comparer
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
        DoesNotExist,

        /// <summary>A matches regex pattern B.</summary>
        MatchesRegex,

        /// <summary>A does not match regex pattern B.</summary>
        DoesNotMatchRegex
    }

    /// <summary>
    /// Static Class used to check if a condition is true or false.
    /// </summary>
    public static class Condition
    {
        /// <summary>
        /// Replaces the values and verifies if a condition is true or false.
        /// </summary>
        /// <param name="left">The left term</param>
        /// <param name="comparer">The comparison operator</param>
        /// <param name="right">The right term</param>
        /// <param name="data">The BotData used for variable replacement</param>
        /// <returns>Whether the comparison is verified or not.</returns>
        public static bool ReplaceAndVerify(string left, Comparer comparer, string right, BotData data)
        {
            return ReplaceAndVerify(new KeycheckCondition() { Left = left, Comparer = comparer, Right = right }, data);
        }

        /// <summary>
        /// Replaces the values and verifies if a condition is true or false.
        /// </summary>
        /// <param name="kcCond">The keycheck condition struct</param>
        /// <param name="data">The BotData used for variable replacement</param>
        /// <returns>Whether the comparison is verified or not.</returns>
        public static bool ReplaceAndVerify(KeycheckCondition kcCond, BotData data)
        {
            var style = NumberStyles.Number | NumberStyles.AllowCurrencySymbol; // Needed when comparing values with a currency symbol
            var provider = new CultureInfo("en-US");
            var L = BlockBase.ReplaceValuesRecursive(kcCond.Left, data); // The left-hand term can accept recursive values like <LIST[*]>
            var r = BlockBase.ReplaceValues(kcCond.Right, data); // The right-hand term cannot

            switch (kcCond.Comparer)
            {
                case Comparer.EqualTo:
                    return L.Any(l => l == r);

                case Comparer.NotEqualTo:
                    return L.Any(l => l != r);

                case Comparer.GreaterThan:
                    return L.Any(l => decimal.Parse(l.Replace(',', '.'), style, provider) > decimal.Parse(r.Replace(',', '.'), style, provider));

                case Comparer.LessThan:
                    return L.Any(l => decimal.Parse(l.Replace(',', '.'), style, provider) < decimal.Parse(r.Replace(',', '.'), style, provider));

                case Comparer.Contains:
                    return L.Any(l => l.Contains(r));

                case Comparer.DoesNotContain:
                    return L.Any(l => !l.Contains(r));

                case Comparer.Exists:
                    return L.Any(l => l != kcCond.Left); // Returns true if any replacement took place

                case Comparer.DoesNotExist:
                    return L.All(l => l == kcCond.Left); // Returns true if no replacement took place

                case Comparer.MatchesRegex:
                    return L.Any(l => Regex.Match(l, r).Success);

                case Comparer.DoesNotMatchRegex:
                    return L.Any(l => !Regex.Match(l, r).Success);

                default:
                    return false;
            }
        }

        /// <summary>
        /// Verifies if a condition is true or false (without replacing the values).
        /// </summary>
        /// <param name="kcCond">The keycheck condition struct</param>
        /// <returns>Whether the comparison is verified or not.</returns>
        public static bool Verify(KeycheckCondition kcCond)
        {
            var style = NumberStyles.Number | NumberStyles.AllowCurrencySymbol; // Needed when comparing values with a currency symbol
            var provider = new CultureInfo("en-US");

            switch (kcCond.Comparer)
            {
                case Comparer.EqualTo:
                    return kcCond.Left == kcCond.Right;

                case Comparer.NotEqualTo:
                    return kcCond.Left != kcCond.Right;

                case Comparer.GreaterThan:
                    return decimal.Parse(kcCond.Left.Replace(',', '.'), style, provider) > decimal.Parse(kcCond.Right.Replace(',', '.'), style, provider);

                case Comparer.LessThan:
                    return decimal.Parse(kcCond.Left.Replace(',', '.'), style, provider) < decimal.Parse(kcCond.Right.Replace(',', '.'), style, provider);

                case Comparer.Contains:
                    return kcCond.Left.Contains(kcCond.Right);

                case Comparer.DoesNotContain:
                    return !kcCond.Left.Contains(kcCond.Right);

                case Comparer.Exists:
                case Comparer.DoesNotExist:
                    throw new NotSupportedException("Exists and DoesNotExist operators are only supported in the ReplaceAndVerify method.");

                case Comparer.MatchesRegex:
                    return Regex.Match(kcCond.Left, kcCond.Right).Success;

                case Comparer.DoesNotMatchRegex:
                    return !Regex.Match(kcCond.Left, kcCond.Right).Success;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Verifies if all the provided conditions are true (after replacing).
        /// </summary>
        /// <param name="conditions">The keycheck conditions</param>
        /// <param name="data">The BotData used for variable replacement</param>
        /// <returns>True if all the conditions are verified.</returns>
        public static bool ReplaceAndVerifyAll(KeycheckCondition[] conditions, BotData data)
        {
            return conditions.All(c => ReplaceAndVerify(c, data));
        }

        /// <summary>
        /// Verifies if all the provided conditions are true (without replacing).
        /// </summary>
        /// <param name="conditions">The keycheck conditions</param>
        /// <returns>True if all the conditions are verified.</returns>
        public static bool VerifyAll(KeycheckCondition[] conditions)
        {
            return conditions.All(c => Verify(c));
        }

        /// <summary>
        /// Verifies if at least one of the provided conditions is true (after replacing).
        /// </summary>
        /// <param name="conditions">The keycheck conditions</param>
        /// <param name="data">The BotData used for variable replacement</param>
        /// <returns>True if any condition is verified.</returns>
        public static bool ReplaceAndVerifyAny(KeycheckCondition[] conditions, BotData data)
        {
            return conditions.Any(c => ReplaceAndVerify(c, data));
        }

        /// <summary>
        /// Verifies if at least one of the provided conditions is true (without replacing).
        /// </summary>
        /// <param name="conditions">The keycheck conditions</param>
        /// <returns>True if any condition is verified.</returns>
        public static bool VerifyAny(KeycheckCondition[] conditions)
        {
            return conditions.Any(c => Verify(c));
        }
    }

    /// <summary>
    /// Represents a condition of a keycheck.
    /// </summary>
    public struct KeycheckCondition
    {
        /// <summary>
        /// The left term.
        /// </summary>
        public string Left;

        /// <summary>
        /// The comparison operator.
        /// </summary>
        public Comparer Comparer;

        /// <summary>
        /// The right term.
        /// </summary>
        public string Right;
    }
}
