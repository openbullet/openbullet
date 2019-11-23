using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;
using Extreme.Net;
using Newtonsoft.Json.Linq;

namespace RuriLib.Utils.Parsing
{
    /// <summary>
    /// Provides parsing methods.
    /// </summary>
    public static class Parse
    {
        /// <summary>
        /// Parses one or more strings from another string based on the left and right delimiters.
        /// </summary>
        /// <param name="input">The string to parse from</param>
        /// <param name="left">The left string delimiter</param>
        /// <param name="right">The right string delimiter</param>
        /// <param name="recursive">Whether to find all the occurrences</param>
        /// <param name="useRegex">Whether to automatically build a Regex pattern and use Regex matching</param>
        /// <returns>The parsed strings.</returns>
        public static IEnumerable<string> LR(string input, string left, string right, bool recursive = false, bool useRegex = false)
        {
            // No L and R = return full input
            if (left == "" && right == "")
            {
                return new string[] { input };
            }

            // L or R not present and not empty = return nothing
            else if (((left != "" && !input.Contains(left)) || (right != "" && !input.Contains(right))))
            {
                return new string[] { };
            }

            var partial = input;
            var pFrom = 0;
            var pTo = 0;
            var list = new List<string>();

            if (recursive)
            {
                if (useRegex)
                {
                    try
                    {
                        var pattern = BuildLRPattern(left, right);
                        MatchCollection mc = Regex.Matches(partial, pattern);
                        foreach (Match m in mc)
                            list.Add(m.Value);
                    }
                    catch { }
                }
                else
                {
                    try
                    {
                        while (left == "" || (partial.Contains(left)) && (right == "" || partial.Contains(right)))
                        {
                            // Search for left delimiter and Calculate offset
                            pFrom = left == "" ? 0 : partial.IndexOf(left) + left.Length;
                            // Move right of offset
                            partial = partial.Substring(pFrom);
                            // Search for right delimiter and Calculate length to parse
                            pTo = right == "" ? (partial.Length - 1) : partial.IndexOf(right);
                            // Parse it
                            var parsed = partial.Substring(0, pTo);
                            list.Add(parsed);
                            // Move right of parsed + right
                            partial = partial.Substring(parsed.Length + right.Length);
                        }
                    }
                    catch { }
                }
            }

            // Non-recursive
            else
            {
                if (useRegex)
                {
                    var pattern = BuildLRPattern(left, right);
                    MatchCollection mc = Regex.Matches(partial, pattern);
                    if (mc.Count > 0) list.Add(mc[0].Value);
                }
                else
                {
                    try
                    {
                        pFrom = left == "" ? 0 : partial.IndexOf(left) + left.Length;
                        partial = partial.Substring(pFrom);
                        pTo = right == "" ? partial.Length : partial.IndexOf(right);
                        list.Add(partial.Substring(0, pTo));
                    }
                    catch { }
                }
            }

            return list;
        }

        /// <summary>
        /// Parses an attribute's value from one or more elements of an HTML page.
        /// </summary>
        /// <param name="input">The HTML page</param>
        /// <param name="selector">The CSS Selector that targets the desired elements</param>
        /// <param name="attribute">The attribute for which you want to parse the value</param>
        /// <param name="index">The index of the element to parse among all the ones selected (if not recursive)</param>
        /// <param name="recursive">Whether to parse from all the elements that match the selector</param>
        /// <returns>The attribute value(s).</returns>
        public static IEnumerable<string> CSS(string input, string selector, string attribute, int index = 0, bool recursive = false)
        {
            HtmlParser parser = new HtmlParser();
            AngleSharp.Html.Dom.IHtmlDocument document = null;
            document = parser.ParseDocument(input);
            var list = new List<string>();

            if (recursive)
            {
                foreach (var element in document.QuerySelectorAll(selector))
                {
                    switch (attribute)
                    {
                        case "innerHTML":
                            list.Add(element.InnerHtml);
                            break;

                        case "outerHTML":
                            list.Add(element.OuterHtml);
                            break;

                        default:
                            foreach (var attr in element.Attributes)
                            {
                                if (attr.Name == attribute)
                                {
                                    list.Add(attr.Value);
                                    break;
                                }
                            }
                            break;
                    }
                }
            }
            else
            {
                switch (attribute)
                {
                    case "innerHTML":
                        list.Add(document.QuerySelectorAll(selector)[index].InnerHtml);
                        break;

                    case "outerHTML":
                        list.Add(document.QuerySelectorAll(selector)[index].OuterHtml);
                        break;

                    default:
                        foreach (var attr in document.QuerySelectorAll(selector)[index].Attributes)
                        {
                            if (attr.Name == attribute)
                            {
                                list.Add(attr.Value);
                                break;
                            }
                        }
                        break;
                }
            }

            return list;
        }

        /// <summary>
        /// Parses a JSON object or array.
        /// </summary>
        /// <param name="input">The serialized JSON object or array</param>
        /// <param name="field">The field for which you want to parse the value</param>
        /// <param name="recursive">Whether to return all the fields matching the name or JToken</param>
        /// <param name="useJToken">Whether to match via JToken</param>
        /// <returns>The value(s) of the parsed field(s).</returns>
        public static IEnumerable<string> JSON(string input, string field, bool recursive = false, bool useJToken = false)
        {
            var list = new List<string>();

            if (useJToken)
            {
                if (recursive)
                {
                    if (input.Trim().StartsWith("["))
                    {
                        JArray json = JArray.Parse(input);
                        var jsonlist = json.SelectTokens(field, false);
                        foreach (var j in jsonlist)
                            list.Add(j.ToString());
                    }
                    else
                    {
                        JObject json = JObject.Parse(input);
                        var jsonlist = json.SelectTokens(field, false);
                        foreach (var j in jsonlist)
                            list.Add(j.ToString());
                    }
                }
                else
                {
                    if (input.Trim().StartsWith("["))
                    {
                        JArray json = JArray.Parse(input);
                        list.Add(json.SelectToken(field, false).ToString());
                    }
                    else
                    {
                        JObject json = JObject.Parse(input);
                        list.Add(json.SelectToken(field, false).ToString());
                    }
                }
            }
            else
            {
                var jsonlist = new List<KeyValuePair<string, string>>();
                parseJSON("", input, jsonlist);
                foreach (var j in jsonlist)
                    if (j.Key == field)
                        list.Add(j.Value);

                if (!recursive && list.Count > 1) list = new List<string>() { list.First() };
            }

            return list;
        }

        /// <summary>
        /// Parses a string via a Regex pattern containing Groups, then returns them according to an output format.
        /// </summary>
        /// <param name="input">The string to parse</param>
        /// <param name="pattern">The Regex pattern containing groups</param>
        /// <param name="output">The output format string, for which [0] will be replaced with the full match, [1] with the first group etc.</param>
        /// <param name="recursive">Whether to make multiple matches</param>
        /// <returns>The parsed string(s).</returns>
        public static IEnumerable<string> REGEX(string input, string pattern, string output, bool recursive = false)
        {
            var list = new List<string>();

            if (recursive)
            {
                var matches = Regex.Matches(input, pattern);
                foreach (Match match in matches)
                {
                    var final = output;
                    for (var i = 0; i < match.Groups.Count; i++) final = final.Replace("[" + i + "]", match.Groups[i].Value);
                    list.Add(final);
                }
            }
            else
            {
                var match = Regex.Match(input, pattern);
                if (match.Success)
                {
                    var final = output;
                    for (var i = 0; i < match.Groups.Count; i++) final = final.Replace("[" + i + "]", match.Groups[i].Value);
                    list.Add(final);
                }
            }

            return list;
        }

        private static string BuildLRPattern(string ls, string rs)
        {
            var left = string.IsNullOrEmpty(ls) ? "^" : Regex.Escape(ls); // Empty LEFT = start of the line
            var right = string.IsNullOrEmpty(rs) ? "$" : Regex.Escape(rs); // Empty RIGHT = end of the line
            return "(?<=" + left + ").+?(?=" + right + ")";
        }

        private static void parseJSON(string A, string B, List<KeyValuePair<string, string>> jsonlist)
        {
            jsonlist.Add(new KeyValuePair<string, string>(A, B));

            if (B.StartsWith("["))
            {
                JArray arr = null;
                try { arr = JArray.Parse(B); } catch { return; }

                foreach (var i in arr.Children())
                    parseJSON("", i.ToString(), jsonlist);
            }

            if (B.Contains("{"))
            {
                JObject obj = null;
                try { obj = JObject.Parse(B); } catch { return; }

                foreach (var o in obj)
                    parseJSON(o.Key, o.Value.ToString(), jsonlist);
            }
        }
    }
}
