using RuriLib.ViewModels;
using System;
using System.Windows.Media;

namespace RuriLib
{
    /// <summary>
    /// How critical a log entry is.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>An informational message.</summary>
        Info,

        /// <summary>A warning message.</summary>
        Warning,

        /// <summary>An error message.</summary>
        Error
    }

    /// <summary>
    /// A single entry in the log of the application.
    /// </summary>
    public class LogEntry : ViewModelBase
    {
        private string logString = "";
        /// <summary>The logged message.</summary>
        public string LogString { get { return logString; } set { logString = value; OnPropertyChanged(); } }

        private Color logColor = Colors.White;
        /// <summary>The color of the logged entry.</summary>
        public Color LogColor { get { return logColor; } set { logColor = value; OnPropertyChanged(); } }

        private DateTime logTime = DateTime.Now;
        /// <summary>The timestamp of the logged entry.</summary>
        public DateTime LogTime { get { return logTime; } set { logTime = value; OnPropertyChanged(); } }

        private LogLevel logLevel = LogLevel.Info;
        /// <summary>The level of the logged entry.</summary>
        public LogLevel LogLevel { get { return logLevel; } set { logLevel = value; OnPropertyChanged(); } }

        private string logComponent = "";
        /// <summary>The component that logged the entry.</summary>
        public string LogComponent { get { return logComponent; } set { logComponent = value; OnPropertyChanged(); } }

        /// <summary>
        /// <para>Creates a Log Entry given a message and a color.</para>
        /// <para>This constructor is used to generate entries for the Bot Log.</para>
        /// <para>The level is Info and the component is an empty string.</para>
        /// </summary>
        /// <param name="logString">The message to log</param>
        /// <param name="logColor">The color of the message</param>
        public LogEntry(string logString, Color logColor)
        {
            LogString = logString;
            LogColor = logColor;
            LogTime = DateTime.Now;
        }

        /// <summary>
        /// <para>Creates a Log Entry given a component, a message and a level.</para>
        /// <para>This constructor is used for logging messages coming from components of the View.</para>
        /// </summary>
        /// <param name="logComponent">The Component that is logging the entry</param>
        /// <param name="logString">The message to log</param>
        /// <param name="logLevel">The level of the log</param>
        public LogEntry(string logComponent, string logString, LogLevel logLevel)
        {
            LogString = logString;
            LogTime = DateTime.Now;
            LogLevel = logLevel;
            LogComponent = logComponent;

            switch (LogLevel)
            {
                case LogLevel.Info:
                    LogColor = Colors.White;
                    break;

                case LogLevel.Warning:
                    LogColor = Colors.Gold;
                    break;

                case LogLevel.Error:
                    LogColor = Colors.Tomato;
                    break;
            }
        }
    }
}
