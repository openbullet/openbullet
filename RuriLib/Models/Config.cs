using Newtonsoft.Json;
using RuriLib.ViewModels;
using System.Collections.Generic;
using System.Linq;
using RuriLib.LS;
using System.Text.RegularExpressions;
using System;

namespace RuriLib
{
    /// <summary>
    /// Represents a configuration in RuriLib.
    /// </summary>
    public class Config : ViewModelBase
    {
        /// <summary>
        /// Creates a Config given the settings and the script.
        /// </summary>
        /// <param name="settings">The settings of the Config</param>
        /// <param name="script">The LoliScript script of the Config</param>
        public Config(ConfigSettings settings, string script)
        {
            Settings = settings;
            Script = script;
        }

        /// <summary>The settings of the Config.</summary>
        public ConfigSettings Settings { get; set; }

        /// <summary>The LoliScript script of the Config.</summary>
        public string Script { get; set; }

        /// <summary>Whether Selenium is being used in any of the blocks of the Config.</summary>
        public bool SeleniumPresent { get { return Script.Contains("NAVIGATE") || Script.Contains("BROWSERACTION") || Script.Contains("ELEMENTACTION") || Script.Contains("EXECUTEJS") || Script.Contains("MOUSEACTION"); } }

        /// <summary>Whether a possibly dangerous JavaScript or IronPython script is present in the Config.</summary>
        public bool DangerousScriptPresent { get { return Script.ToUpper().Contains("BEGIN SCRIPT"); } }

        /// <summary>Whether captcha solving blocks are present in the Config.</summary>
        public bool CaptchasNeeded { get { return Regex.Match(Script, "(^|\n)(#[^ ]* )?(RE)?CAPTCHA").Success; } }

        /// <summary>Whether bypasscf blocks are present in the Config.</summary>
        public bool HasCFBypass { get { return Script.Contains("BYPASSCF"); } }

        /// <summary>The amount of blocks of the Config.</summary>
        public int BlocksAmount { get { return Script.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Where(l => BlockParser.IsBlock(l)).Count(); } }

        /// <summary>The joined representation of the allowed WordlistTypes.</summary>
        public string AllowedWordlists { get { return $"{Settings.AllowedWordlist1} | {Settings.AllowedWordlist2}"; } }

        /// <summary>The pretty representation of the time when the Config was last modified.</summary>
        public string LastModifiedString { get { return $"{Settings.LastModified.ToShortDateString()} {Settings.LastModified.ToShortTimeString()}"; } }
    }
}
