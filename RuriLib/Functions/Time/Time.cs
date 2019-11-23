using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuriLib.Functions.Time
{
    /// <summary>
    /// Provides methods to work with dates and times.
    /// </summary>
    public static class Time
    {
        /// <summary>
        /// Converts a DateTime to unix time seconds.
        /// </summary>
        /// <param name="dateTime">The DateTime to convert</param>
        /// <returns>The seconds that passed since Jan 1st 1970.</returns>
        public static long ToUnixTimeSeconds(this DateTime dateTime)
        {
            return (long)dateTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }

        /// <summary>
        /// Converts a DateTime to unix time milliseconds.
        /// </summary>
        /// <param name="dateTime">The DateTime to convert</param>
        /// <returns>The milliseconds that passed since Jan 1st 1970.</returns>
        public static long ToUnixTimeMilliseconds(this DateTime dateTime)
        {
            return (long)dateTime.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
        }

        /// <summary>
        /// Converts a time string to a DateTime.
        /// </summary>
        /// <param name="time">The string to convert</param>
        /// <param name="format">The string's format</param>
        /// <returns>A DateTime.</returns>
        public static DateTime ToDateTime(this string time, string format)
        {
            return DateTime.ParseExact(time, format, new CultureInfo("en-US"), DateTimeStyles.AllowWhiteSpaces);
        }

        /// <summary>
        /// Converts a unix time to a DateTime.
        /// </summary>
        /// <param name="unixTime">The unix time in seconds or milliseconds</param>
        /// <returns>A DateTime.</returns>
        public static DateTime ToDateTime(this double unixTime)
        {
            // If the unixTime is in seconds, convert it to milliseconds
            if (unixTime < 10000000000) unixTime *= 1000;
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return dtDateTime.AddMilliseconds(unixTime).ToUniversalTime();
        }

        /// <summary>
        /// Converts a DateTime to an ISO8601 time string.
        /// </summary>
        /// <param name="dateTime">A DateTime</param>
        /// <returns>An ISO8601 time string.</returns>
        public static string ToISO8601(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffZ");
        }
    }
}
