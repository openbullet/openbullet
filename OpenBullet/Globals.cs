using OpenBullet.ViewModels;
using RuriLib;
using RuriLib.Models;
using RuriLib.Runner;
using RuriLib.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace OpenBullet
{
    public enum Components
    {
        Main,
        RunnerManager,
        Runner,
        ProxyManager,
        WordlistManager,
        HitsDB,
        ConfigManager,
        Stacker,
        OtherOptions,
        Settings,
        ListGenerator,
        SeleniumTools,
        ComboSuite,
        About
    }

    public static class Globals
    {
        // Version
        public static string obVersion = "1.1.4";

        // Main Window
        public static MainWindow mainWindow;

        // Log Window
        public static LogWindow logWindow;
        public static OBLog log = new OBLog();

        // Constant file paths
        public static string dataBaseFile = @"DB/OpenBullet.db";
        public static string obSettingsFile = @"Settings/OBSettings.json";
        public static string rlSettingsFile = @"Settings/RLSettings.json";
        public static string envFile = @"Settings/Environment.ini";
        public static string licenseFile = @"Settings/License.txt";
        public static string logFile = @"Log.txt";
        public static string configFolder = @"Configs";

        // Settings
        public static OBSettingsViewModel obSettings;
        public static RLSettingsViewModel rlSettings;
        public static EnvironmentSettings environment;
        public static Random random = new Random();

        // Runners
        public static ObservableCollection<RunnerViewModel> Runners = new ObservableCollection<RunnerViewModel>();

        // Logging Methods
        public static void Log(Components component, LogLevel level, string message, bool prompt = false, int timeout = 0)
        {
            if (prompt)
            {
                if (timeout == 0)
                {
                    MessageBox.Show(message, level.ToString());
                }
                else
                {
                    AutoClosingMessageBox.Show(message, level.ToString(), timeout * 1000);
                }
            }

            if (!CanLog()) return;
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                var entry = new LogEntry(component.ToString(), message, level);
                InsertEntry(entry);
                LogToFile(entry);
            }));
        }

        public static void LogInfo(Components component, string message, bool prompt = false, int timeout = 0)
        {
            Log(component, LogLevel.Info, message, prompt, timeout);
        }

        public static void LogWarning(Components component, string message, bool prompt = false, int timeout = 0)
        {
            Log(component, LogLevel.Warning, message, prompt, timeout);
        }

        public static void LogError(Components component, string message, bool prompt = false, int timeout = 0)
        {
            Log(component, LogLevel.Error, message, prompt, timeout);
        }

        private static bool CanLog()
        {
            try
            {
                return obSettings.General.EnableLogging;
            }
            catch { return false; }
        }

        private static void InsertEntry(LogEntry entry)
        {
            try
            {
                log.List.Insert(0, entry);
            }
            catch { }
        }

        private static void LogToFile(LogEntry entry)
        {
            try
            {
                if(obSettings.General.LogToFile)
                    File.AppendAllText(logFile, $"[{entry.LogTime}] ({entry.LogLevel}) {entry.LogComponent} - " + entry.LogString + Environment.NewLine);
            }
            catch { }
        }

        public static System.Windows.Media.Color GetColor(string propertyName)
        {
            try { return ((SolidColorBrush)App.Current.Resources[propertyName]).Color; }
            catch { return ((SolidColorBrush)App.Current.Resources["ForegroundMain"]).Color; }
        }

        public static SolidColorBrush GetBrush(string propertyName)
        {
            try { return (SolidColorBrush)App.Current.Resources[propertyName]; }
            catch { return (SolidColorBrush)App.Current.Resources["ForegroundMain"]; }
        }
    }

    // Source: https://stackoverflow.com/questions/14522540/close-a-messagebox-after-several-seconds
    public class AutoClosingMessageBox
    {
        System.Threading.Timer _timeoutTimer;
        string _caption;
        AutoClosingMessageBox(string text, string caption, int timeout)
        {
            _caption = caption;
            _timeoutTimer = new System.Threading.Timer(OnTimerElapsed,
                null, timeout, System.Threading.Timeout.Infinite);
            using (_timeoutTimer)
                MessageBox.Show(text, caption);
        }
        public static void Show(string text, string caption, int timeout)
        {
            new AutoClosingMessageBox(text, caption, timeout);
        }
        void OnTimerElapsed(object state)
        {
            IntPtr mbWnd = FindWindow("#32770", _caption); // lpClassName is #32770 for MessageBox
            if (mbWnd != IntPtr.Zero)
                SendMessage(mbWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            _timeoutTimer.Dispose();
        }
        const int WM_CLOSE = 0x0010;
        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
    }
}
