using System.Collections.Generic;
using System.Reflection;

namespace RuriLib.ViewModels
{
    /// <summary>
    /// The level of detail of a bot's status.
    /// </summary>
    public enum BotsDisplayMode
    {
        /// <summary>No information is displayed.</summary>
        None,

        /// <summary>Every information is displayed.</summary>
        Everything,

        /// <summary>Only information about the end result is displayed.</summary>
        EndResultOnly
    }

    /// <summary>
    /// Provides settings for general purposes.
    /// </summary>
    public class SettingsGeneral : ViewModelBase
    {
        private int _waitTime = 100;
        /// <summary>The amount of time the Bot needs to wait after completing a check, in milliseconds.</summary>
        public int WaitTime { get { return _waitTime; } set { _waitTime = value; OnPropertyChanged(); } }

        private int _requestsTimeout = 10;
        /// <summary>The maximum amount of time to wait for a response from the server for any HTTP request, in seconds.</summary>
        public int RequestTimeout { get { return _requestsTimeout; } set { _requestsTimeout = value; OnPropertyChanged(); } }

        private int _maxHits = 0;
        /// <summary>The maximum number of hits before the Runner automatically stops (0 for infinite).</summary>
        public int MaxHits { get { return _maxHits; } set { _maxHits = value; OnPropertyChanged(); } }

        private BotsDisplayMode _botsDisplayMode = BotsDisplayMode.Everything;
        /// <summary>The rate and detail of information to display in the Bot Status column.</summary>
        public BotsDisplayMode BotsDisplayMode { get { return _botsDisplayMode; } set { _botsDisplayMode = value; OnPropertyChanged(); } }

        private bool _enableBotLog = false;
        /// <summary>Whether to keep the LogBuffer from BotData stored for future use (slows down the Runner).</summary>
        public bool EnableBotLog { get { return _enableBotLog; } set { _enableBotLog = value; OnPropertyChanged(); } }

        /// <summary>
        /// Resets the properties to their default value.
        /// </summary>
        public void Reset()
        {
            SettingsGeneral def = new SettingsGeneral();
            IList<PropertyInfo> props = new List<PropertyInfo>(typeof(SettingsGeneral).GetProperties());

            foreach (PropertyInfo prop in props)
            {
                prop.SetValue(this, prop.GetValue(def, null));
            }
        }
    }
}