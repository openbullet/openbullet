using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuriLib.Functions.Formats
{
    /// <summary>
    /// Provides methods for encoding data in various formats.
    /// </summary>
    public static class Encode
    {
        /// <summary>
        /// Encodes a string to base64.
        /// </summary>
        /// <param name="plainText">The string to encode</param>
        /// <returns>The base64-encoded string</returns>
        public static string ToBase64(this string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        /// <summary>
        /// Decodes a base64-encoded string.
        /// </summary>
        /// <param name="base64EncodedData">The base64-encoded string</param>
        /// <returns>The decoded string</returns>
        public static string FromBase64(this string base64EncodedData)
        {
            var toDecode = base64EncodedData.Replace(".", "");
            var remainder = toDecode.Length % 4;
            if (remainder != 0) toDecode = toDecode.PadRight(toDecode.Length + (4 - remainder), '=');
            var base64EncodedBytes = System.Convert.FromBase64String(toDecode);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
