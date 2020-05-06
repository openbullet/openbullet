using Extreme.Net;
using OpenQA.Selenium.Remote;
using RuriLib.Enums;
using RuriLib.Models;
using RuriLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Net.Sockets;
using System.Windows.Media;

namespace RuriLib
{
    /// <summary>
    /// The status of the Bot. While the status is NONE or SUCCESS the Bot keeps checking the script until the end, otherwise it stops.
    /// </summary>
    public enum BotStatus
    {
        /// <summary>The initial status of the bot. If a bot ends its execution with this stauts, the data line will be registered as ToCheck.</summary>
        NONE,

        /// <summary>Something wrong happened while processing a block.</summary>
        ERROR,

        /// <summary>The data line will be registered as a Hit.</summary>
        SUCCESS,

        /// <summary>The data line will be registered as a fail.</summary>
        FAIL,

        /// <summary>The data line will be retried and the proxy will be banned.</summary>
        BAN,

        /// <summary>The data line will be retried.</summary>
        RETRY,

        /// <summary>The data line will be registered as a Custom.</summary>
        CUSTOM
    }

    /// <summary>
    /// Class that holds all the variables needed for checking a single data line.
    /// </summary>
    public class BotData
    {
        /// <summary>A random number generator.</summary>
        public Random random;

        /// <summary>The Status of the Bot.</summary>
        public BotStatus Status { get; set; }

        /// <summary>The Custom Status name in case the Status was set to Custom.</summary>
        public string CustomStatus = "CUSTOM";

        /// <summary>The Status of the Bot as a string.</summary>
        public string StatusString { get { return Status == BotStatus.CUSTOM ? CustomStatus : Status.ToString(); } }

        /// <summary>The id of the current Bot.</summary>
        public int BotNumber { get; set; }

        /// <summary>The selenium webdriver currently being used.</summary>
        public RemoteWebDriver Driver { get; set; }

        /// <summary>Whether the browser is open or not.</summary>
        public bool BrowserOpen { get; set; }

        /// <summary>A dictionary of object that can be used for keeping session-based objects through different blocks.</summary>
        public Dictionary<string, object> CustomObjects { get; set; }

        /// <summary>The wrapped data line that needs to be checked.</summary>
        public CData Data { get; set; }

        /// <summary>The proxy currently being used (null if none).</summary>
        public CProxy Proxy { get; set; }

        /// <summary>Whether we should use proxies or not.</summary>
        public bool UseProxies { get; set; }

        /// <summary>The Global RuriLib settings valid for any Config.</summary>
        public RLSettingsViewModel GlobalSettings { get; set; }

        /// <summary>The settings for the current Config.</summary>
        public ConfigSettings ConfigSettings { get; set; }

        /// <summary>The remaining balance on the captcha service account.</summary>
        public decimal Balance { get; set; }

        /// <summary>The paths of the Screenshots saved by selenium.</summary>
        public List<string> Screenshots { get; set; }

        #region Response
        /// <summary>The return Address of the HttpResponse.</summary>
        public string Address {
            get { return Variables.Get("ADDRESS").Value; }
            set { Variables.SetHidden("ADDRESS", value); }
        }

        /// <summary>The HTTP code of the HttpResponse.</summary>
        public string ResponseCode
        {
            get { return Variables.Get("RESPONSECODE").Value; }
            set { Variables.SetHidden("RESPONSECODE", value); }
        }

        /// <summary>The dictionary of headers of the HttpResponse.</summary>
        public Dictionary<string, string> ResponseHeaders
        {
            get { return Variables.Get("HEADERS").Value; }
            set { Variables.SetHidden("HEADERS", value); }
        }

        /// <summary>The dictionary of cookies of the HttpResponse.</summary>
        public Dictionary<string, string> Cookies
        {
            get { return Variables.Get("COOKIES").Value; }
            set { Variables.SetHidden("COOKIES", value); }
        }

        /// <summary>The source of the HttpResponse.</summary>
        public string ResponseSource
        {
            get { return Variables.Get("SOURCE").Value; }
            set { Variables.SetHidden("SOURCE", value); }
        }
        #endregion

        // Variables and capture
        /// <summary>The local variables of the Bot.</summary>
        public VariableList Variables { get; set; }

        /// <summary>The global variables shared between all the Bots. They can be accessed or set by any bot.</summary>
        public VariableList GlobalVariables { get; set; }

        /// <summary>The global cookies shared between all the Bots. They are set in the local cookie jar at the start of the check.</summary>
        public Dictionary<string, string> GlobalCookies { get; set; }

        // Log
        /// <summary>The detailed log of the last block that was run.</summary>
        public List<LogEntry> LogBuffer { get; set; }

        /// <summary>Whether this object is being used for a Debugger or for the Runner.</summary>
        public bool IsDebug { get; set; }

        /// <summary>
        /// Creates a BotData object given some parameters.
        /// </summary>
        /// <param name="globalSettings">The global RuriLib settings</param>
        /// <param name="configSettings">The settings for the current Config</param>
        /// <param name="data">The wrapped data line to check</param>
        /// <param name="proxy">The proxy to use (set to null if none)</param>
        /// <param name="useProxies">Whether to use the proxy for requests</param>
        /// <param name="random">A reference to the global random generator</param>
        /// <param name="botNumber">The number of the bot that is creating this object</param>
        /// <param name="isDebug">Whether this object is created from a Debugger or from a Runner</param>
        public BotData(RLSettingsViewModel globalSettings, ConfigSettings configSettings, CData data, CProxy proxy, bool useProxies, Random random, int botNumber = 0, bool isDebug = true)
        {
            Data = data;
            Proxy = proxy;
            UseProxies = useProxies;
            this.random = new Random(random.Next(0, int.MaxValue)); // Create a new local RNG seeded with a random seed from the global RNG
            Status = BotStatus.NONE;
            BotNumber = BotNumber;
            GlobalSettings = globalSettings;
            ConfigSettings = configSettings;
            Balance = 0;
            Screenshots = new List<string>();
            Variables = new VariableList();

            // Populate the Variables list with hidden variables
            Address = "";
            ResponseCode = "0";
            ResponseSource = "";
            Cookies = new Dictionary<string, string>();
            ResponseHeaders = new Dictionary<string, string>();
            try { foreach (var v in Data.GetVariables(ConfigSettings.EncodeData)) Variables.Set(v); } catch { }
            
            GlobalVariables = new VariableList();
            GlobalCookies = new CookieDictionary();            
            LogBuffer = new List<LogEntry>();
            Driver = null;
            BrowserOpen = false;
            IsDebug = isDebug;
            BotNumber = botNumber;

            CustomObjects = new Dictionary<string, object>();
        }

        /// <summary>
        /// Adds a new LogEntry to the LogBuffer.
        /// </summary>
        /// <param name="entry">The LogEntry to add</param>
        public void Log(LogEntry entry)
        {
            if (GlobalSettings.General.EnableBotLog || IsDebug)
                LogBuffer.Add(entry);
        }

        /// <summary>
        /// Adds a new message to the LogBuffer with white text.
        /// </summary>
        /// <param name="message">The message to log</param>
        public void Log(string message)
        {
            Log(new LogEntry(message, Colors.White));
        }

        /// <summary>
        /// Adds multiple new LogEntry objects to the LogBuffer.
        /// </summary>
        /// <param name="list">The list of log entries to add</param>
        public void LogRange(List<LogEntry> list)
        {
            if (GlobalSettings.General.EnableBotLog || IsDebug)
                LogBuffer.AddRange(list);
        }

        /// <summary>
        /// Logs an empty line to the LogBuffer.
        /// </summary>
        public void LogNewLine()
        {
            LogBuffer.Add(new LogEntry("", Colors.White));
        }

        /// <summary>
        /// Retrieves a custom object from the 
        /// </summary>
        /// <param name="key">The key of the object in the dictionary</param>
        /// <returns>The object or null if the key was not found</returns>
        public object GetCustomObject(string key)
        {
            return CustomObjects.ContainsKey(key) ? CustomObjects[key] : null;
        }
    }
}
