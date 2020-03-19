using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuriLib
{
    /// <summary>
    /// Extension methods for lists.
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// Randomizes the elements in a list.
        /// </summary>
        /// <param name="list">The list to shuffle</param>
        /// <param name="rng">The random number generator</param>
        public static void Shuffle<T>(this IList<T> list, Random rng)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }

    /// <summary>
    /// Extension methods for strings.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Replaces literal values of \n, \r\n and \t with the actual escape codes.
        /// </summary>
        /// <param name="str">The string to unescape</param>
        /// <param name="useEnvNewLine">Whether to unescape both \n and \r\n with the Environment.NewLine</param>
        /// <returns>The string with unescaped escape sequences.</returns>
        public static string Unescape(this string str, bool useEnvNewLine = false)
        {
            var stringBuilder = new StringBuilder(str)
                .Replace(@"\t", "\t");

            if (useEnvNewLine)
            {
                stringBuilder
                    .Replace(@"\n", Environment.NewLine)
                    .Replace(@"\r\n", Environment.NewLine);
            }
            else
            {
                stringBuilder
                    .Replace(@"\n", "\n")
                    .Replace(@"\r\n", "\r\n");
            }

            return stringBuilder.ToString();
        }
    }
}
