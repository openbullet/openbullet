using Newtonsoft.Json;
using RuriLib.Models;
using RuriLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace RuriLib
{
    /// <summary>
    /// A generic block used to process information in RuriLib.
    /// </summary>
    public abstract class BlockBase : ViewModelBase
    {
        private string label = "BASE";
        /// <summary>The label of the block.</summary>
        public string Label { get { return label; } set { label = value; OnPropertyChanged(); } }

        private bool disabled = false;
        /// <summary>Whether the block should be skipped or processed.</summary>
        public bool Disabled { get { return disabled; } set { disabled = value; OnPropertyChanged(); } }

        /// <summary>Whether the block is a selenium-related block.</summary>
        [JsonIgnore]
        public bool IsSelenium { get { return GetType().ToString().StartsWith("S"); } }

        /// <summary>Whether the block is a captcha-related block.</summary>
        [JsonIgnore]
        public bool IsCaptcha { get { return GetType() == typeof(BlockImageCaptcha) || GetType() == typeof(BlockRecaptcha); } }

        #region Virtual methods
        /// <summary>
        /// Builds a block from a line of LoliScript code.
        /// </summary>
        /// <param name="line">The line of LoliScript code</param>
        /// <returns>The parsed block object</returns>
        public virtual BlockBase FromLS(string line)
        {
            throw new Exception("Cannot Convert to the abstract class BlockBase");
        }

        /// <summary>
        /// Builds a block from multiple lines of LoliScript code.
        /// </summary>
        /// <param name="lines">The lines of LoliScript code</param>
        /// <returns>The parsed block object</returns>
        public virtual BlockBase FromLS(List<string> lines)
        {
            throw new Exception("Cannot Convert from the abstract class BlockBase");
        }

        /// <summary>
        /// Converts the block to LoliScript code.
        /// </summary>
        /// <param name="indent"></param>
        /// <returns></returns>
        public virtual string ToLS(bool indent = true)
        {
            throw new Exception("Cannot Convert from the abstract class BlockBase");
        }

        /// <summary>
        /// Executes the actual block logic.
        /// </summary>
        /// <param name="data">The BotData needed for variable replacement</param>
        public virtual void Process(BotData data)
        {
            // Clear log buffer
            data.LogBuffer.Clear();
            data.Log(new LogEntry(string.Format("<--- Executing Block {0} --->", Label),Colors.Orange));
        }
        #endregion

        #region Variable Replacement
        /// <summary>
        /// Replaces variables recursively, expanding lists or dictionaries with jolly indices.
        /// </summary>
        /// <param name="input">The string to replace variables into</param>
        /// <param name="data">The BotData needed for variable replacement</param>
        /// <returns>An array of values obtained replacing the original input with each of the possible values of the first List or Dictionary variable found</returns>
        public static List<string> ReplaceValuesRecursive(string input, BotData data)
        {
            var toReplace = new List<string>();

            // Regex parse the syntax <LIST[*]>
            var matches = Regex.Matches(input, @"<([^\[]*)\[\*\]>");
            var variables = new List<CVar>();

            foreach (Match m in matches)
            {
                var name = m.Groups[1].Value;

                // Retrieve the dict
                var variable = data.Variables.Get(name);
                if (variable == null)
                {
                    variable = data.GlobalVariables.Get(name);
                    if (variable == null) continue;
                }

                if (variable.Type == CVar.VarType.List)
                {
                    variables.Add(variable);
                }
            }

            // If there's no corresponding variable, just readd the input string and proceed with normal replacement
            if (variables.Count > 0)
            {
                var max = variables.OrderBy(v => v.Value.Count).Last().Value.Count;
                for (var i = 0; i < max; i++)
                {
                    var replaced = input;
                    foreach(var variable in variables)
                    {
                        var list = (List<string>)variable.Value;
                        if (list.Count > i)
                        {
                            replaced = replaced.Replace($"<{variable.Name}[*]>", list[i]);
                        }
                        else
                        {
                            replaced = replaced.Replace($"<{variable.Name}[*]>", "NULL");
                        }
                    }
                    toReplace.Add(replaced);
                }
                goto END;
            }

            // Regex parse the syntax <DICT(*)> (wildcard key -> returns list of all values)
            var match = Regex.Match(input, @"<([^\(]*)\(\*\)>");

            if (match.Success)            
            {
                var full = match.Groups[0].Value;
                var name = match.Groups[1].Value;

                // Retrieve the list
                var dict = data.Variables.GetDictionary(name);
                if (dict == null) dict = data.GlobalVariables.GetDictionary(name);

                // If there's no corresponding variable, just readd the input string and proceed with normal replacement
                if (dict == null) toReplace.Add(input);
                else
                {
                    foreach (var item in dict)
                        toReplace.Add(input.Replace(full, item.Value));
                }
                goto END;
            }

            // Regex parse the syntax <DICT{*}> (wildcard value -> returns list of all keys)
            match = Regex.Match(input, @"<([^\{]*)\{\*\}>");

            if (match.Success)
            {
                var full = match.Groups[0].Value;
                var name = match.Groups[1].Value;

                // Retrieve the dict
                var dict = data.Variables.GetDictionary(name);
                if (dict == null) dict = data.GlobalVariables.GetDictionary(name);

                // If there's no corresponding variable, just readd the input string and proceed with normal replacement
                if (dict == null) toReplace.Add(input);
                else
                {
                    foreach (var item in dict)
                        toReplace.Add(input.Replace(full, item.Key));
                }
                goto END;
            }

            // If no other match was a success, it means there's no recursive value and we simply add the input to the list
            toReplace.Add(input);

            END:
            // Now for each item in the list, do the normal replacement and return the replaced list of strings
            return toReplace.Select(i => ReplaceValues(i, data)).ToList();
        }

        /// <summary>
        /// Replaces variables in a given input string.
        /// </summary>
        /// <param name="input">The string to replace variables into</param>
        /// <param name="data">The BotData needed for variable replacement</param>
        /// <returns>The string where variables have been replaced</returns>
        public static string ReplaceValues(string input, BotData data)
        {
            if (!input.Contains("<") && !input.Contains(">")) return input;

            var previous = "";
            var output = input;

            do
            {
                previous = output;

                // Replace all the fixed quantities (this needs to go away inside BotData.cs, initialized as hidden vars)
                output = output.Replace("<INPUT>", data.Data.Data);
                output = output.Replace("<STATUS>", data.Status.ToString());
                output = output.Replace("<BOTNUM>", data.BotNumber.ToString());
                output = output.Replace("<RETRIES>", data.Data.Retries.ToString());
                if (data.Proxy != null)
                    output = output.Replace("<PROXY>", data.Proxy.Proxy);

                // Get all the inner (max. 1 level of nesting) variables
                var matches = Regex.Matches(output, @"<([^<>]*)>");
                
                foreach(Match match in matches)
                {
                    var full = match.Groups[0].Value;
                    var m = match.Groups[1].Value;

                    // Parse the variable name
                    var name = Regex.Match(m, @"^[^\[\{\(]*").Value;

                    // Try to get the variable (first local, then global, then if none was found go to the next iteration)
                    // We don't throw an error here because it could be some HTML or XML code e.g. <br> that triggers this, and we dont' want to spam the user with unneeded errors
                    var v = data.Variables.Get(name);
                    if (v == null) v = data.GlobalVariables.Get(name);
                    if (v == null) continue;

                    // Parse the arguments
                    var args = m.Replace(name, "");

                    switch (v.Type)
                    {
                        case CVar.VarType.Single:
                            output = output.Replace(full, v.Value);
                            break;

                        case CVar.VarType.List:

                            // If it's just the list name, replace it with its string representation
                            if (string.IsNullOrEmpty(args))
                            {
                                output = output.Replace(full, v.ToString());
                                break;
                            }

                            var index = 0;
                            int.TryParse(ParseArguments(args, '[', ']')[0], out index);
                            var item = v.GetListItem(index); // Can return null
                            if (item != null) output = output.Replace(full, item);
                            break;

                        case CVar.VarType.Dictionary:
                            
                            if (args.Contains("(") && args.Contains(")"))
                            {
                                var dicKey = ParseArguments(args, '(', ')')[0];
                                try { output = output.Replace(full, v.GetDictValue(dicKey)); } catch { }
                            }
                            else if (args.Contains("{") && args.Contains("}"))
                            {
                                var dicVal = ParseArguments(args, '{', '}')[0];
                                try { output = output.Replace(full, v.GetDictKey(dicVal)); } catch { }
                            }
                            else // If it's just the dictionary name, replace it with its string representation
                            {
                                output = output.Replace(full, v.ToString());
                                break;
                            }
                            break;
                    }
                }
            }
            while (input.Contains("<") && input.Contains(">") && output != previous);

            return output;
        }
        #endregion

        /// <summary>
        /// Parses an argument between two bracket delimiters.
        /// </summary>
        /// <param name="input">The string to parse the argument from</param>
        /// <param name="delimL">The left bracket delimiter</param>
        /// <param name="delimR">The right bracket delimiter</param>
        /// <returns>The argument between the delimiters</returns>
        public static List<string> ParseArguments(string input, char delimL, char delimR)
        {
            var output = new List<string>();
            var matches = Regex.Matches(input, @"\" + delimL + @"([^\" + delimR + @"]*)\" + delimR);
            foreach (Match match in matches) output.Add(match.Groups[1].Value);
            return output;
        }

        /// <summary>
        /// Updates the ADDRESS and SOURCE variables basing on the selenium-driven browser's URL bar and page source.
        /// </summary>
        /// <param name="data">The BotData containing the driver and the variables</param>
        public static void UpdateSeleniumData(BotData data)
        {
            data.Address = data.Driver.Url;
            data.ResponseSource = data.Driver.PageSource;
        }

        #region Variable Insertion
        /// <summary>
        /// Adds a single variable with the given value.
        /// </summary>
        /// <param name="data">The BotData used for variable replacement and insertion</param>
        /// <param name="isCapture">Whether the variable should be marked for Capture</param>
        /// <param name="value">The value of the variable</param>
        /// <param name="variableName">The name of the variable to create</param>
        /// <param name="prefix">The string to add at the start of the value</param>
        /// <param name="suffix">The string to add at the end of the value</param>
        /// <param name="urlEncode">Whether to URLencode the values before creating the variables</param>
        /// <param name="createEmpty">Whether to create an empty (single) variable if the list of values is empty</param>
        public static void InsertVariable(BotData data, bool isCapture, string value, string variableName,
            string prefix = "", string suffix = "", bool urlEncode = false, bool createEmpty = true)
        {
            InsertVariable(data, isCapture, false, new string[] { value }, variableName, prefix, suffix, urlEncode, createEmpty);
        }

        /// <summary>
        /// Adds a list variable with the given value.
        /// </summary>
        /// <param name="data">The BotData used for variable replacement and insertion</param>
        /// <param name="isCapture">Whether the variable should be marked for Capture</param>
        /// <param name="values">The list of values</param>
        /// <param name="variableName">The name of the variable to create</param>
        /// <param name="prefix">The string to add at the start of the value</param>
        /// <param name="suffix">The string to add at the end of the value</param>
        /// <param name="urlEncode">Whether to URLencode the values before creating the variables</param>
        /// <param name="createEmpty">Whether to create an empty (single) variable if the list of values is empty</param>
        public static void InsertVariable(BotData data, bool isCapture, IEnumerable<string> values, string variableName,
            string prefix = "", string suffix = "", bool urlEncode = false, bool createEmpty = true)
        {
            InsertVariable(data, isCapture, true, values, variableName, prefix, suffix, urlEncode, createEmpty);
        }

        /// <summary>
        /// Adds a single or list variable with the given value.
        /// </summary>
        /// <param name="data">The BotData used for variable replacement and insertion</param>
        /// <param name="isCapture">Whether the variable should be marked for Capture</param>
        /// <param name="recursive">Whether the variable to add should be a list or a single value</param>
        /// <param name="values">The list of values. In case recursive is set to false, only the first value of the list will be taken.</param>
        /// <param name="variableName">The name of the variable to create</param>
        /// <param name="prefix">The string to add at the start of the value</param>
        /// <param name="suffix">The string to add at the end of the value</param>
        /// <param name="urlEncode">Whether to URLencode the values before creating the variables</param>
        /// <param name="createEmpty">Whether to create an empty (single) variable if the list of values is empty</param>
        internal static void InsertVariable(BotData data, bool isCapture, bool recursive, IEnumerable<string> values, string variableName, 
            string prefix = "", string suffix = "", bool urlEncode = false, bool createEmpty = true)
        {
            var list = values.Select(v => ReplaceValues(prefix, data) + v.Trim() + ReplaceValues(suffix, data)).ToList();
            if (urlEncode) list = list.Select(v => Uri.EscapeDataString(v)).ToList();

            CVar variable = null;
            if (recursive)
            {
                if (list.Count == 0)
                {
                    if (createEmpty)
                    {
                        variable = new CVar(variableName, list, isCapture);
                    }
                }
                else
                {
                    variable = new CVar(variableName, list, isCapture);
                }
            }
            else
            {
                if (list.Count == 0)
                {
                    if (createEmpty)
                    {
                        variable = new CVar(variableName, "", isCapture);
                    }
                }
                else
                {
                    variable = new CVar(variableName, list.First(), isCapture);
                }
            }

            // If we don't want to save empty captures, and it's a capture, and the list is either an empty string or a list made of an empty string
            if (!data.ConfigSettings.SaveEmptyCaptures && isCapture && 
                (list.Count == 0 || list.Count > 0 && string.IsNullOrWhiteSpace(list.First())))
            {
                variable = null;
            }

            if (variable != null)
            {
                data.Variables.Set(variable);
                data.Log(new LogEntry("Parsed variable" + " | Name: " + variable.Name + " | Value: " + variable.ToString() + Environment.NewLine, isCapture ? Colors.OrangeRed : Colors.Gold));
            }
            else
            {
                data.Log(new LogEntry("Could not parse any data. The variable was not created.", Colors.White));
            }
        }
        #endregion

        #region File Utilities
        /// <summary>
        /// Truncates a string to fit a given size and adds ' [...]' (5 characters) at the end to display that the string would be longer.
        /// </summary>
        /// <param name="input">The string to truncate</param>
        /// <param name="max">The maximum length of the string</param>
        /// <returns>The truncated string, or the same string if its length was not exceeding the maximum size</returns>
        public static string TruncatePretty(string input, int max)
        {
            input = input.Replace("\r\n", "").Replace("\n", "");
            if (input.Length < max) return input;
            return input.Substring(0, max) + " [...]";
        }
        #endregion
    }
}
