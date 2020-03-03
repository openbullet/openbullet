using OpenBullet.ViewModels;
using RuriLib.Interfaces;
using System;

namespace OpenBullet
{
    public static class OB
    {
        public static IApplication App => new OpenBulletApp()
        {
            RunnerManager = RunnerManager,
            ProxyManager = ProxyManager,
            ProxyChecker = ProxyManager,
            WordlistManager = WordlistManager,
            ConfigManager = ConfigManager,
            HitsDB = HitsDB,
            Settings = Settings,
            Logger = Logger,
            Alerter = Alerter
        };

        public static string Version => "1.2.0";

        public static Random random = new Random();

        // Windows
        // TODO: Remove these from here, everything should only depend on the ViewModels not on the Views!
        public static MainWindow MainWindow { get; set; }
        public static LogWindow LogWindow { get; set; }

        // ViewModels
        public static RunnerManagerViewModel RunnerManager { get; set; }
        public static ProxyManagerViewModel ProxyManager { get; set; }
        public static WordlistManagerViewModel WordlistManager { get; set; }
        public static ConfigManagerViewModel ConfigManager { get; set; }
        public static StackerViewModel Stacker { get; set; }
        public static HitsDBViewModel HitsDB { get; set; }
        public static Alerter Alerter { get; set; } = new Alerter();
        public static LoggerViewModel Logger { get; set; } = new LoggerViewModel();
        public static GlobalSettings Settings { get; set; } = new GlobalSettings();
        public static OBSettingsViewModel OBSettings { get; set; }

        // Constant file paths
        public static readonly string dataBaseFile = @"DB/OpenBullet.db";
        public static readonly string dataBaseBackupFile = @"DB/OpenBullet-BackupCopy.db";
        public static readonly string obSettingsFile = @"Settings/OBSettings.json";
        public static readonly string rlSettingsFile = @"Settings/RLSettings.json";
        public static readonly string envFile = @"Settings/Environment.ini";
        public static readonly string licenseFile = @"Settings/License.txt";
        public static readonly string logFile = @"Log.txt";
        public static readonly string configFolder = @"Configs";
    }
}
