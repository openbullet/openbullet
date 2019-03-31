using RuriLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenBullet.ViewModels
{
    public class OBSettingsGeneral : ViewModelBase
    {
        private bool displayLoliScriptOnLoad = false;
        public bool DisplayLoliScriptOnLoad { get { return displayLoliScriptOnLoad; } set { displayLoliScriptOnLoad = value; OnPropertyChanged(); } }
        private bool recommendedBots = true;
        public bool RecommendedBots { get { return recommendedBots; } set { recommendedBots = value; OnPropertyChanged(); } }
        private bool changeRunnerInterface = false;
        public bool ChangeRunnerInterface { get { return changeRunnerInterface; } set { changeRunnerInterface = value; OnPropertyChanged(); } }
        private bool disableQuitWarning = false;
        public bool DisableQuitWarning { get { return disableQuitWarning; } set { disableQuitWarning = value; OnPropertyChanged(); } }
        private bool disableNotSavedWarning = false;
        public bool DisableNotSavedWarning { get { return disableNotSavedWarning; } set { disableNotSavedWarning = value; OnPropertyChanged(); } }
        private string defaultAuthor = "";
        public string DefaultAuthor { get { return defaultAuthor; } set { defaultAuthor = value; OnPropertyChanged(); } }
        private bool enableLogging = false;
        public bool EnableLogging { get { return enableLogging; } set { enableLogging = value; OnPropertyChanged(); } }
        private bool logToFile = false;
        public bool LogToFile { get { return logToFile; } set { logToFile = value; OnPropertyChanged(); } }
        private bool liveConfigUpdates = false;
        public bool LiveConfigUpdates { get { return liveConfigUpdates; } set { liveConfigUpdates = value; OnPropertyChanged(); } }
        private bool disableHTMLView = false;
        public bool DisableHTMLView { get { return disableHTMLView; } set { disableHTMLView = value; OnPropertyChanged(); } }
        private bool alwaysOnTop = false;
        public bool AlwaysOnTop { get { return alwaysOnTop; } set { alwaysOnTop = value; OnPropertyChanged(); } }
        private bool autoCreateRunner = false;
        public bool AutoCreateRunner { get { return autoCreateRunner; } set { autoCreateRunner = value; OnPropertyChanged(); } }
        private bool persistDebuggerLog = false;
        public bool PersistDebuggerLog { get { return persistDebuggerLog; } set { persistDebuggerLog = value; OnPropertyChanged(); } }
        private bool disableSyntaxHelper = false;
        public bool DisableSyntaxHelper { get { return disableSyntaxHelper; } set { disableSyntaxHelper = value; OnPropertyChanged(); } }
        private bool displayCapturesLast = false;
        public bool DisplayCapturesLast { get { return displayCapturesLast; } set { displayCapturesLast = value; OnPropertyChanged(); } }
    }
}
