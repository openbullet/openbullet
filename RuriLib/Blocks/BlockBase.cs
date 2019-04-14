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

            // Regex parse the syntax <LIST[*]> (only the first one! This doesn't support multiple arrays because they can have different sizes)
            var match = Regex.Match(input, @"<([^\[]*)\[\*\]>");

            if (match.Success)
            {
                var full = match.Groups[0].Value;
                var name = match.Groups[1].Value;

                // Retrieve the dict
                var list = data.Variables.GetList(name);
                if (list == null) list = data.GlobalVariables.GetList(name);

                // If there's no corresponding variable, just readd the input string and proceed with normal replacement
                if (list == null) toReplace.Add(input);
                else
                {
                    foreach (var item in list)
                        toReplace.Add(input.Replace(full, item));
                }
                goto END;
            }

            // Regex parse the syntax <DICT(*)> (wildcard key -> returns list of all values)
            match = Regex.Match(input, @"<([^\(]*)\(\*\)>");

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
        public static void InsertVariables(BotData data, bool isCapture, bool recursive, List<string> values, string variableName, string prefix, string suffix)
        {
            var list = values.Select(v => ReplaceValues(prefix, data) + v.Trim() + ReplaceValues(suffix, data));
            CVar variable;
            if (recursive) variable = new CVar(variableName, list.ToList(), isCapture);
            else variable = new CVar(variableName, list.First(), isCapture);
            data.Variables.Set(variable);
            data.Log(new LogEntry("Parsed variable" + " | Name: " + variable.Name + " | Value: " + variable.ToString() + Environment.NewLine, isCapture ? Colors.OrangeRed : Colors.Gold));
        }

        #region File Utilities
        /// <summary>
        /// Saves a Selenium screenshot to a file with automatically generated name.
        /// </summary>
        /// <param name="screenshot">The Selenium screenshot</param>
        /// <param name="data">The BotData used for path creation</param>
        public static void SaveScreenshot(OpenQA.Selenium.Screenshot screenshot, BotData data)
        {
            var path = MakeScreenshotPath(data);
            data.Screenshots.Add(path);
            screenshot.SaveAsFile(path);
        }

        /// <summary>
        /// Saves a screenshot to a file with automatically generated name.
        /// </summary>
        /// <param name="screenshot">The bitmap image</param>
        /// <param name="data">The BotData used for path creation</param>
        public static void SaveScreenshot(Bitmap screenshot, BotData data)
        {
            var path = MakeScreenshotPath(data);
            data.Screenshots.Add(path);
            screenshot.Save(path);
        }

        /// <summary>
        /// Builds the path for the screenshot file.
        /// </summary>
        /// <param name="data">The BotData for path creation</param>
        /// <returns>The path of the file to save the screenshot to</returns>
        private static string MakeScreenshotPath(BotData data)
        {
            var folderName = MakeValidFileName(data.ConfigSettings.Name);
            var originalFilename = MakeValidFileName(data.Data.Data);
            // Check if you have to make the folder
            if (!Directory.Exists($"Screenshots\\{folderName}")) Directory.CreateDirectory($"Screenshots\\{folderName}");

            // Save the file inside the folder
            var filename = GetFirstAvailableFileName($"Screenshots\\{folderName}\\", originalFilename, "bmp");
            return $"Screenshots\\{folderName}\\{filename}";
        }

        /// <summary>
        /// Gets the first available name in the given folder by incrementing a number at the end of the filename.
        /// </summary>
        /// <param name="basePath">The path to the folder</param>
        /// <param name="fileName">The name of the file without numbers at the end</param>
        /// <param name="extension">The extension of the file</param>
        /// <returns>The first available filename (including extension)</returns>
        public static string GetFirstAvailableFileName(string basePath, string fileName, string extension)
        {
            int i;
            for (i = 1; File.Exists(basePath + fileName + i + "." + extension); i++) { }
            return fileName + i + "." + extension;
        }

        /// <summary>
        /// Fixes the filename to be compatible with the filesystem indicization.
        /// </summary>
        /// <param name="name">The name of the file</param>
        /// <param name="underscore">Whether to replace the unallowed characters with an underscore instead of removing them</param>
        /// <returns>The valid filename ready to be saved to disk</returns>
        public static string MakeValidFileName(string name, bool underscore = true)
        {
            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return Regex.Replace(name, invalidRegStr, underscore ? "_" : "");
        }

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
