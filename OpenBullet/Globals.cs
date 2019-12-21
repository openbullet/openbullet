using OpenBullet.ViewModels;
using RuriLib;
using RuriLib.Models;
using RuriLib.Runner;
using RuriLib.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace OpenBullet
{
    public static class Globals
    {
        // Version
        public static string obVersion = "1.1.5";

        // Main Window
        public static MainWindow mainWindow;

        // Log Window
        public static LogWindow logWindow;
        public static LoggerViewModel logger = new LoggerViewModel();

        // Constant file paths
        public static string dataBaseFile = @"DB/OpenBullet.db";
        public static string dataBaseBackupFile = @"DB/OpenBullet-BackupCopy.db";
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

        public static Color GetColor(string propertyName)
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
}
