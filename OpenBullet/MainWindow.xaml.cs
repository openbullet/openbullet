using OpenBullet.ViewModels;
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
        public Runner CurrentRunnerPage { get; set; }
        public ProxyManager ProxyManagerPage { get; set; }
        public WordlistManager WordlistManagerPage { get; set; }
        public Configs ConfigsPage { get; set; }
        public HitsDB HitsDBPage { get; set; }
        public Settings OBSettingsPage { get; set; }
        public Tools ToolsPage { get; set; }
        public About AboutPage { get; set; }
        public System.Drawing.Rectangle Bounds { get; private set; }

        private string title = $"OpenBullet {Globals.obVersion}";
        private bool maximized = false;
        System.Windows.Point _startPosition;
        bool _isResizing = false;

        private BackgroundWorker ruriGuard = new BackgroundWorker();
        private BackgroundWorker ruriKiller = new BackgroundWorker();
        private BackgroundWorker ruriScout = new BackgroundWorker();

        public MainWindow()
        {
            // Clean or create log file
            File.WriteAllText(Globals.logFile, "");

            InitializeComponent();

            Title = title;
            titleLabel.Content = title;

            // Set global reference to this window
            Globals.mainWindow = this;

            // Make sure all folders are there or recreate them
            var folders = new string[] { "Captchas", "ChromeExtensions", "Configs", "DB", "Screenshots", "Settings", "Sounds", "Wordlists" };
            foreach(var folder in folders.Select(f => System.IO.Path.Combine(Directory.GetCurrentDirectory(), f)))
            {
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
            }

            // Initialize Environment Settings
            try
            {
                Globals.environment = IOManager.ParseEnvironmentSettings(Globals.envFile);
            }
            catch
            {
                MessageBox.Show("Could not find / parse the Environment Settings file. Please fix the issue and try again.");
                Environment.Exit(0);
            }

            if (Globals.environment.WordlistTypes.Count == 0 || Globals.environment.CustomKeychains.Count == 0)
            {
                MessageBox.Show("At least one WordlistType and one CustomKeychain must be defined in the Environment Settings file.");
                Environment.Exit(0);
            }

            // Initialize Settings
            Globals.rlSettings = new RLSettingsViewModel();
            Globals.obSettings = new OBSettingsViewModel();

            // Create / Load Settings
            if (!File.Exists(Globals.rlSettingsFile)) {
                MessageBox.Show("RuriLib Settings file not found, generating a default one");
                Globals.LogWarning(Components.Main, "RuriLib Settings file not found, generating a default one");
                IOManager.SaveSettings(Globals.rlSettingsFile, Globals.rlSettings);
                Globals.LogInfo(Components.Main, $"Created the default RuriLib Settings file {Globals.rlSettingsFile}");
            }
            else {
                Globals.rlSettings = IOManager.LoadSettings(Globals.rlSettingsFile);
                Globals.LogInfo(Components.Main, "Loaded the existing RuriLib Settings file");
            }

            if (!File.Exists(Globals.obSettingsFile))
            {
                MessageBox.Show("OpenBullet Settings file not found, generating a default one");
                Globals.LogWarning(Components.Main, "OpenBullet Settings file not found, generating a default one");
                OBIOManager.SaveSettings(Globals.obSettingsFile, Globals.obSettings);
                Globals.LogInfo(Components.Main, $"Created the default OpenBullet Settings file {Globals.obSettingsFile}");
            }
            else
            {
                Globals.obSettings = OBIOManager.LoadSettings(Globals.obSettingsFile);
                Globals.LogInfo(Components.Main, "Loaded the existing OpenBullet Settings file");
            }

            Topmost = Globals.obSettings.General.AlwaysOnTop;

            RunnerManagerPage = new RunnerManager(Globals.obSettings.General.AutoCreateRunner);
            if (Globals.obSettings.General.AutoCreateRunner)
                CurrentRunnerPage = RunnerManagerPage.vm.Runners.FirstOrDefault().Page;
            Globals.LogInfo(Components.Main, "Initialized RunnerManager");
            ProxyManagerPage = new ProxyManager();
            Globals.LogInfo(Components.Main, "Initialized ProxyManager");
            WordlistManagerPage = new WordlistManager();
            Globals.LogInfo(Components.Main, "Initialized WordlistManager");
            ConfigsPage = new Configs();
            Globals.LogInfo(Components.Main, "Initialized ConfigManager");
            HitsDBPage = new HitsDB();
            Globals.LogInfo(Components.Main, "Initialized HitsDB");
            OBSettingsPage = new Settings();
            Globals.LogInfo(Components.Main, "Initialized Settings");
            ToolsPage = new Tools();
            Globals.LogInfo(Components.Main, "Initialized Tools");
            AboutPage = new About();            

            menuOptionRunner_MouseDown(this, null);

            // Re-enable this before release
            // (new SplashWindow()).ShowDialog();

            if (Globals.obSettings.Themes.EnableSnow)
                Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var t = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(10000 / Globals.obSettings.Themes.SnowAmount) };
            t.Tick += (s, ea) => Snow();
            t.Start();
        }

        private void Snow()
        {
            if (snowBuffer >= 100) {
                int i = 0;
                while(i < Root.Children.Count)
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
                    c.Foreground = Globals.GetBrush("ForegroundMain");
                }
                catch { }
            }
            ((Label)sender).Foreground = Globals.GetBrush("ForegroundMenuSelected");
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
            var active = RunnerManagerPage.vm.Runners.Count(r => r.Runner.Busy);
            if (!Globals.obSettings.General.DisableQuitWarning || active > 0)
            {
                Globals.LogWarning(Components.Main, "Prompting quit confirmation");
                
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

            if (!Globals.obSettings.General.DisableNotSavedWarning && !Globals.mainWindow.ConfigsPage.ConfigManagerPage.CheckSaved())
            {
                Globals.LogWarning(Components.Main, "Config not saved, prompting quit confirmation");
                if (MessageBox.Show("The Config in Stacker wasn't saved.\nAre you sure you want to quit?",
                    "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                    return false;
            }
            Globals.LogInfo(Components.Main, "Quit sequence initiated");
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
            if(maximized)
            {
                this.Width = 800;
                this.Height = 620;
                Left = 0;
                Top = 0;
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
        
        private void maximizePanel_MouseDC(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {

                if (maximized)
                {
                    this.Width = 800;
                    this.Height = 620;
                    Left = 0;
                    Top = 0;
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
            Globals.LogInfo(Components.Main, "Acquired screenshot");
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
            if(Globals.logWindow == null)
            {
                Globals.logWindow = new LogWindow();
                Globals.logWindow.Show();
            }
            else
            {
                Globals.logWindow.Show();
            }
        }

        public void SetStyle()
        {
            try
            {
                var brush = Globals.GetBrush("BackgroundMain");

                if (!Globals.obSettings.Themes.UseImage)
                {   
                    Background = brush;
                    Main.Background = brush;
                }
                else
                {
                    // BACKGROUND
                    if (File.Exists(Globals.obSettings.Themes.BackgroundImage))
                    {
                        var bbrush = new ImageBrush(new BitmapImage(new Uri(Globals.obSettings.Themes.BackgroundImage)));
                        bbrush.Opacity = (double)((double)Globals.obSettings.Themes.BackgroundImageOpacity / (double)100);
                        Background = bbrush;
                    }
                    else
                    {
                        Background = brush;
                    }

                    // LOGO
                    if (File.Exists(Globals.obSettings.Themes.BackgroundLogo))
                    {
                        var lbrush = new ImageBrush(new BitmapImage(new Uri(Globals.obSettings.Themes.BackgroundLogo)));
                        lbrush.AlignmentX = AlignmentX.Center;
                        lbrush.AlignmentY = AlignmentY.Center;
                        lbrush.Stretch = Stretch.None;
                        lbrush.Opacity = (double)((double)Globals.obSettings.Themes.BackgroundImageOpacity / (double)100);
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
