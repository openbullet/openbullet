using OpenBullet.ViewModels;
using OpenBullet.Views.Main;
using OpenBullet.Views.Main.Settings;
using OpenBullet.Views.Main.Runner;
using RuriLib;
using RuriLib.ViewModels;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using OpenBullet.Plugins;
using System.Collections.Generic;
using OpenBullet.Views.UserControls;
using OpenBullet.Views.StackerBlocks;
using RuriLib.LS;

namespace OpenBullet
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Random rand = new Random();
        private int snowBuffer = 0;

        public RunnerManager RunnerManagerPage { get; set; }
        // TODO: Do not create a different View for each RunnerInstance, but instead just replace the vm!
        public Runner CurrentRunnerPage { get; set; }
        public ProxyManager ProxyManagerPage { get; set; }
        public WordlistManager WordlistManagerPage { get; set; }
        public ConfigsSection ConfigsPage { get; set; }
        public HitsDB HitsDBPage { get; set; }
        public Settings OBSettingsPage { get; set; }
        public ToolsSection ToolsPage { get; set; }
        public PluginsSection PluginsPage { get; set; }
        public About AboutPage { get; set; }
        public Rectangle Bounds { get; private set; }

        private bool maximized = false;
        System.Windows.Point _startPosition;
        bool _isResizing = false;

        public MainWindow()
        {
            OB.MainWindow = this;

            // Clean or create log file
            File.WriteAllText(OB.logFile, "");

            InitializeComponent();

            var title = $"OpenBullet {OB.Version}";
            Title = title;
            titleLabel.Content = title;

            // Make sure all folders are there or recreate them
            var folders = new string[] { "Captchas", "ChromeExtensions", "Configs", "DB", "Hits", "Plugins", "Screenshots", "Settings", "Sounds", "Wordlists" };
            foreach (var folder in folders.Select(f => Path.Combine(Directory.GetCurrentDirectory(), f)))
            {
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
            }

            // Initialize Environment Settings
            try
            {
                OB.Settings.Environment = IOManager.ParseEnvironmentSettings(OB.envFile);
            }
            catch
            {
                OB.Logger.LogError(Components.Main, 
                    "Could not find / parse the Environment Settings file. Please fix the issue and try again.", true);
                Environment.Exit(0);
            }

            if (OB.Settings.Environment.WordlistTypes.Count == 0 || OB.Settings.Environment.CustomKeychains.Count == 0)
            {
                OB.Logger.LogError(Components.Main, 
                    "At least one WordlistType and one CustomKeychain must be defined in the Environment Settings file.", true);
                Environment.Exit(0);
            }

            // Initialize Settings
            OB.Settings.RLSettings = new RLSettingsViewModel();
            OB.Settings.ProxyManagerSettings = new ProxyManagerSettings();
            OB.OBSettings = new OBSettingsViewModel();

            // Create / Load Settings
            if (!File.Exists(OB.rlSettingsFile))
            {
                MessageBox.Show("RuriLib Settings file not found, generating a default one");
                OB.Logger.LogWarning(Components.Main, "RuriLib Settings file not found, generating a default one");
                IOManager.SaveSettings(OB.rlSettingsFile, OB.Settings.RLSettings);
                OB.Logger.LogInfo(Components.Main, $"Created the default RuriLib Settings file {OB.rlSettingsFile}");
            }
            else
            {
                OB.Settings.RLSettings = IOManager.LoadSettings<RLSettingsViewModel>(OB.rlSettingsFile);
                OB.Logger.LogInfo(Components.Main, "Loaded the existing RuriLib Settings file");
            }

            if (!File.Exists(OB.proxyManagerSettingsFile))
            {
                OB.Logger.LogWarning(Components.Main, "Proxy manager Settings file not found, generating a default one");
                OB.Settings.ProxyManagerSettings.ProxySiteUrls.Add(OB.defaultProxySiteUrl);
                OB.Settings.ProxyManagerSettings.ActiveProxySiteUrl = OB.defaultProxySiteUrl;
                OB.Settings.ProxyManagerSettings.ProxyKeys.Add(OB.defaultProxyKey);
                OB.Settings.ProxyManagerSettings.ActiveProxyKey = OB.defaultProxyKey;
                IOManager.SaveSettings(OB.proxyManagerSettingsFile, OB.Settings.ProxyManagerSettings);
                OB.Logger.LogInfo(Components.Main, $"Created the default proxy manager Settings file {OB.proxyManagerSettingsFile}");
            }
            else
            {
                OB.Settings.ProxyManagerSettings = IOManager.LoadSettings<ProxyManagerSettings>(OB.proxyManagerSettingsFile);
                OB.Logger.LogInfo(Components.Main, "Loaded the existing proxy manager Settings file");
            }

            if (!File.Exists(OB.obSettingsFile))
            {
                MessageBox.Show("OpenBullet Settings file not found, generating a default one");
                OB.Logger.LogWarning(Components.Main, "OpenBullet Settings file not found, generating a default one");
                OBIOManager.SaveSettings(OB.obSettingsFile, OB.OBSettings);
                OB.Logger.LogInfo(Components.Main, $"Created the default OpenBullet Settings file {OB.obSettingsFile}");
            }
            else
            {
                OB.OBSettings = OBIOManager.LoadSettings(OB.obSettingsFile);
                OB.Logger.LogInfo(Components.Main, "Loaded the existing OpenBullet Settings file");
            }

            // If there is no DB backup or if it's more than 1 day old, back up the DB
            try
            {
                if (OB.OBSettings.General.BackupDB &&
                    (!File.Exists(OB.dataBaseBackupFile) || 
                    (File.Exists(OB.dataBaseBackupFile) && ((DateTime.Now - File.GetCreationTime(OB.dataBaseBackupFile)).TotalDays > 1))))
                {
                    // Check that the DB is not corrupted by accessing a random collection. If this fails, an exception will be thrown.
                    using (var db = new LiteDB.LiteDatabase(OB.dataBaseFile))
                    {
                        var coll = db.GetCollection<RuriLib.Models.CProxy>("proxies");
                    }

                    // Delete the old file and copy over the new one
                    File.Delete(OB.dataBaseBackupFile);
                    File.Copy(OB.dataBaseFile, OB.dataBaseBackupFile);
                    OB.Logger.LogInfo(Components.Main, "Backed up the DB");
                }
            }
            catch (Exception ex)
            {
                OB.Logger.LogError(Components.Main, $"Could not backup the DB: {ex.Message}");
            }

            Topmost = OB.OBSettings.General.AlwaysOnTop;

            // Load Plugins
            var (plugins, blockPlugins) = Loader.LoadPlugins(OB.pluginsFolder);
            OB.BlockPlugins = blockPlugins.ToList();

            // Set mappings
            OB.BlockMappings = new List<(Type, Type, System.Windows.Media.Color)>()
            {
                ( typeof(BlockBypassCF),        typeof(PageBlockBypassCF),          Colors.DarkSalmon ),
                ( typeof(BlockImageCaptcha),    typeof(PageBlockCaptcha),           Colors.DarkOrange ),
                ( typeof(BlockReportCaptcha),   typeof(PageBlockReportCaptcha),     Colors.DarkOrange ),
                ( typeof(BlockFunction),        typeof(PageBlockFunction),          Colors.YellowGreen ),
                ( typeof(BlockKeycheck),        typeof(PageBlockKeycheck),          Colors.DodgerBlue ),
                ( typeof(BlockLSCode),          typeof(PageBlockLSCode),            Colors.White ),
                ( typeof(BlockParse),           typeof(PageBlockParse),             Colors.Gold ),
                ( typeof(BlockRecaptcha),       typeof(PageBlockRecaptcha),         Colors.Turquoise ),
                ( typeof(BlockSolveCaptcha),    typeof(PageBlockSolveCaptcha),      Colors.Turquoise ),
                ( typeof(BlockRequest),         typeof(PageBlockRequest),           Colors.LimeGreen ),
                ( typeof(BlockTCP),             typeof(PageBlockTCP),               Colors.MediumPurple ),
                ( typeof(BlockUtility),         typeof(PageBlockUtility),           Colors.Wheat ),
                ( typeof(SBlockBrowserAction),  typeof(PageSBlockBrowserAction),    Colors.Green ),
                ( typeof(SBlockElementAction),  typeof(PageSBlockElementAction),    Colors.Firebrick ),
                ( typeof(SBlockExecuteJS),      typeof(PageSBlockExecuteJS),        Colors.Indigo ),
                ( typeof(SBlockNavigate),       typeof(PageSBlockNavigate),         Colors.RoyalBlue )
            };

            // Add block plugins to mappings
            foreach (var plugin in blockPlugins)
            {
                try
                {
                    var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(plugin.Color);
                    OB.BlockMappings.Add((plugin.GetType(), typeof(BlockPluginPage), color));
                    BlockParser.BlockMappings.Add(plugin.Name, plugin.GetType());
                    OB.Logger.LogInfo(Components.Main, $"Initialized {plugin.Name} block plugin");
                }
                catch 
                {
                    OB.Logger.LogError(Components.Main, $"The color {plugin.Color} in block plugin {plugin.Name} is invalid", true);
                    Environment.Exit(0);
                }
            }

            // ViewModels
            OB.RunnerManager = new RunnerManagerViewModel();
            OB.ProxyManager = new ProxyManagerViewModel();
            OB.WordlistManager = new WordlistManagerViewModel();
            OB.ConfigManager = new ConfigManagerViewModel();
            OB.HitsDB = new HitsDBViewModel();

            // Views
            RunnerManagerPage = new RunnerManager();
            
            // If we create first runner and there was no session to restore
            if (OB.OBSettings.General.AutoCreateRunner & !OB.RunnerManager.RestoreSession())
            {
                var firstRunner = OB.RunnerManager.Create();
                CurrentRunnerPage = OB.RunnerManager.RunnersCollection.FirstOrDefault().View;
            }
                
            OB.Logger.LogInfo(Components.Main, "Initialized RunnerManager");
            ProxyManagerPage = new ProxyManager();
            OB.Logger.LogInfo(Components.Main, "Initialized ProxyManager");
            WordlistManagerPage = new WordlistManager();
            OB.Logger.LogInfo(Components.Main, "Initialized WordlistManager");
            ConfigsPage = new ConfigsSection();
            OB.Logger.LogInfo(Components.Main, "Initialized ConfigManager");
            HitsDBPage = new HitsDB();
            OB.Logger.LogInfo(Components.Main, "Initialized HitsDB");
            OBSettingsPage = new Settings();
            OB.Logger.LogInfo(Components.Main, "Initialized Settings");
            ToolsPage = new ToolsSection();
            OB.Logger.LogInfo(Components.Main, "Initialized Tools");
            PluginsPage = new PluginsSection(plugins);
            OB.Logger.LogInfo(Components.Main, "Initialized Plugins");
            AboutPage = new About();

            menuOptionRunner_MouseDown(this, null);

            var width = OB.OBSettings.General.StartingWidth;
            var height = OB.OBSettings.General.StartingHeight;
            if (width > 800) Width = width;
            if (height > 600) Height = height;

            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            if (OB.OBSettings.Themes.EnableSnow)
                Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var t = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(10000 / OB.OBSettings.Themes.SnowAmount) };
            t.Tick += (s, ea) => Snow();
            t.Start();
        }

        private void Snow()
        {
            if (snowBuffer >= 100)
            {
                int i = 0;
                while (i < Root.Children.Count)
                {
                    // Remove first snowflake you find (oldest) before putting another one so there are max 100 snowflakes on screen
                    if (Root.Children[i].GetType() == typeof(Snowflake)) { Root.Children.RemoveAt(i); break; }
                    i++;
                }
            }

            var x = rand.Next(-500, (int)Root.ActualWidth - 100);
            var y = -100;
            var s = rand.Next(5, 15);

            var flake = new Snowflake
            {
                Width = s,
                Height = s,
                RenderTransform = new TranslateTransform
                {
                    X = x,
                    Y = y
                },
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                IsHitTestVisible = false
            };

            Grid.SetColumn(flake, 1);
            Grid.SetRow(flake, 2);
            Root.Children.Add(flake);

            var d = TimeSpan.FromSeconds(rand.Next(1, 4));

            x += rand.Next(100, 500);
            var ax = new DoubleAnimation { To = x, Duration = d };
            flake.RenderTransform.BeginAnimation(TranslateTransform.XProperty, ax);

            y = (int)Root.ActualHeight + 200;
            var ay = new DoubleAnimation { To = y, Duration = d };
            flake.RenderTransform.BeginAnimation(TranslateTransform.YProperty, ay);

            snowBuffer++;
        }

        public void ShowRunnerManager()
        {
            CurrentRunnerPage = null;
            Main.Content = RunnerManagerPage;
        }

        public void ShowRunner(Runner page)
        {
            CurrentRunnerPage = page;
            Main.Content = page;
        }

        #region Menu Options MouseDown Events
        public void menuOptionRunner_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (CurrentRunnerPage == null) Main.Content = RunnerManagerPage;
            else Main.Content = CurrentRunnerPage;
            menuOptionSelected(menuOptionRunner);
        }

        private void menuOptionProxyManager_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Main.Content = ProxyManagerPage;
            menuOptionSelected(menuOptionProxyManager);
        }

        private void menuOptionWordlistManager_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Main.Content = WordlistManagerPage;
            menuOptionSelected(menuOptionWordlistManager);
        }

        private void menuOptionConfigCreator_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Main.Content = ConfigsPage;
            menuOptionSelected(menuOptionConfigCreator);
        }

        private void menuOptionHitsDatabase_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Main.Content = HitsDBPage;
            menuOptionSelected(menuOptionHitsDatabase);
        }

        private void menuOptionTools_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Main.Content = ToolsPage;
            menuOptionSelected(menuOptionTools);
        }

        private void menuOptionPlugins_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Main.Content = PluginsPage;
            menuOptionSelected(menuOptionPlugins);
        }

        private void menuOptionSettings_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Main.Content = OBSettingsPage;
            menuOptionSelected(menuOptionSettings);
        }

        private void menuOptionAbout_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Main.Content = AboutPage;
            menuOptionSelected(menuOptionAbout);
        }

        private void menuOptionSelected(object sender)
        {
            foreach (var child in topMenu.Children)
            {
                try
                {
                    var c = (Label)child;
                    c.Foreground = Utils.GetBrush("ForegroundMain");
                }
                catch { }
            }
            ((Label)sender).Foreground = Utils.GetBrush("ForegroundMenuSelected");
        }
        #endregion

        private void quitPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            quitPanel.Background = new SolidColorBrush(Colors.DarkRed);
        }

        private void quitPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            quitPanel.Background = new SolidColorBrush(Colors.Transparent);
        }

        private void quitPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (CheckOnQuit()) Environment.Exit(0);
        }

        private bool CheckOnQuit()
        {
            var active = OB.RunnerManager.RunnersCollection.Count(r => r.ViewModel.Busy);
            if (!OB.OBSettings.General.DisableQuitWarning || active > 0)
            {
                OB.Logger.LogWarning(Components.Main, "Prompting quit confirmation");

                if (active == 0)
                {
                    if (MessageBox.Show($"Are you sure you want to quit?",
                    "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                        return false;
                }
                else
                {
                    if (MessageBox.Show($"There are {active} active runners. Are you sure you want to quit?",
                    "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                        return false;
                }
            }

            if (!OB.OBSettings.General.DisableNotSavedWarning && !OB.MainWindow.ConfigsPage.ConfigManagerPage.CheckSaved())
            {
                OB.Logger.LogWarning(Components.Main, "Config not saved, prompting quit confirmation");
                if (MessageBox.Show("The Config in Stacker wasn't saved.\nAre you sure you want to quit?",
                    "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                    return false;
            }
            
            OB.Logger.LogInfo(Components.Main, "Saving RunnerManager session to the database");
            OB.RunnerManager.SaveSession();

            OB.Logger.LogInfo(Components.Main, "Quit sequence initiated");
            return true;
        }

        private void maximizePanel_MouseEnter(object sender, MouseEventArgs e)
        {
            maximizePanel.Background = new SolidColorBrush(Colors.DimGray);
        }

        private void maximizePanel_MouseLeave(object sender, MouseEventArgs e)
        {
            maximizePanel.Background = new SolidColorBrush(Colors.Transparent);
        }

        private void maximizePanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (maximized)
            {
                Rect workArea = SystemParameters.WorkArea;
                this.Width = OB.OBSettings.General.StartingWidth;
                this.Height = OB.OBSettings.General.StartingHeight;
                Left = (workArea.Width - this.Width) / 2 + workArea.Left;
                Top = (workArea.Height - this.Height) / 2 + workArea.Top;
                maximized = false;
                WindowState = WindowState.Normal;
            }
            else
            {
                this.Width = SystemParameters.WorkArea.Width;
                this.Height = SystemParameters.WorkArea.Height;
                Left = 0;
                Top = 0;
                maximized = true;
                WindowState = WindowState.Normal;
            }
        }

        private void minimizePanel_MouseEnter(object sender, MouseEventArgs e)
        {
            minimizePanel.Background = new SolidColorBrush(Colors.DimGray);
        }

        private void minimizePanel_MouseLeave(object sender, MouseEventArgs e)
        {
            minimizePanel.Background = new SolidColorBrush(Colors.Transparent);
        }

        private void minimizePanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try { WindowState = WindowState.Minimized; } catch { }
        }

        private void titleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {

                if (maximized)
                {
                    Rect workArea = SystemParameters.WorkArea;
                    this.Width = OB.OBSettings.General.StartingWidth;
                    this.Height = OB.OBSettings.General.StartingHeight;
                    Left = (workArea.Width - this.Width) / 2 + workArea.Left;
                    Top = (workArea.Height - this.Height) / 2 + workArea.Top;
                    maximized = false;
                    WindowState = WindowState.Normal;
                }
                else
                {
                    this.Width = SystemParameters.WorkArea.Width;
                    this.Height = SystemParameters.WorkArea.Height;
                    Left = 0;
                    Top = 0;
                    maximized = true;
                    WindowState = WindowState.Normal;
                }
            }
        }

        private void dragPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void gripImage_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.Capture(gripImage))
            {
                _isResizing = true;
                _startPosition = Mouse.GetPosition(this);
            }
        }

        private void gripImage_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_isResizing)
            {
                System.Windows.Point currentPosition = Mouse.GetPosition(this);
                double diffX = currentPosition.X - _startPosition.X;
                double diffY = currentPosition.Y - _startPosition.Y;
                Width = Width + diffX;
                Height = Height + diffY;
                _startPosition = currentPosition;
            }
        }

        private void gripImage_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isResizing == true)
            {
                _isResizing = false;
                Mouse.Capture(null);
            }
        }

        private void screenshotImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var bitmap = CopyScreen((int)Width, (int)Height, (int)Top, (int)Left);
            Clipboard.SetImage(bitmap);
            GetBitmap(bitmap).Save("screenshot.jpg", ImageFormat.Jpeg);
            OB.Logger.LogInfo(Components.Main, "Acquired screenshot");
        }

        private static BitmapSource CopyScreen(int width, int height, int top, int left)
        {
            using (var screenBmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (var bmpGraphics = Graphics.FromImage(screenBmp))
                {
                    bmpGraphics.CopyFromScreen(left, top, 0, 0, screenBmp.Size);
                    return Imaging.CreateBitmapSourceFromHBitmap(
                        screenBmp.GetHbitmap(),
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                }
            }
        }

        private static Bitmap GetBitmap(BitmapSource source)
        {
            Bitmap bmp = new Bitmap(
              source.PixelWidth,
              source.PixelHeight,
              System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            BitmapData data = bmp.LockBits(
              new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmp.Size),
              ImageLockMode.WriteOnly,
              System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            source.CopyPixels(
              Int32Rect.Empty,
              data.Scan0,
              data.Height * data.Stride,
              data.Stride);
            bmp.UnlockBits(data);
            return bmp;
        }

        private void logImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (OB.LogWindow == null)
            {
                OB.LogWindow = new LogWindow();
                OB.LogWindow.Show();
            }
            else
            {
                OB.LogWindow.Show();
            }
        }

        public void SetStyle()
        {
            try
            {
                var brush = Utils.GetBrush("BackgroundMain");

                if (!OB.OBSettings.Themes.UseImage)
                {
                    Background = brush;
                    Main.Background = brush;
                }
                else
                {
                    // BACKGROUND
                    if (File.Exists(OB.OBSettings.Themes.BackgroundImage))
                    {
                        var bbrush = new ImageBrush(new BitmapImage(new Uri(OB.OBSettings.Themes.BackgroundImage)));
                        bbrush.Opacity = (double)((double)OB.OBSettings.Themes.BackgroundImageOpacity / (double)100);
                        Background = bbrush;
                    }
                    else
                    {
                        Background = brush;
                    }

                    // LOGO
                    if (File.Exists(OB.OBSettings.Themes.BackgroundLogo))
                    {
                        var lbrush = new ImageBrush(new BitmapImage(new Uri(OB.OBSettings.Themes.BackgroundLogo)));
                        lbrush.AlignmentX = AlignmentX.Center;
                        lbrush.AlignmentY = AlignmentY.Center;
                        lbrush.Stretch = Stretch.None;
                        lbrush.Opacity = (double)((double)OB.OBSettings.Themes.BackgroundImageOpacity / (double)100);
                        Main.Background = lbrush;
                    }
                    else
                    {
                        Main.Background = new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/"
                            + Assembly.GetExecutingAssembly().GetName().Name
                            + ";component/"
                            + "Images/Themes/empty.png", UriKind.Absolute)));
                    }
                }
            }
            catch { }
        }
    }
}
