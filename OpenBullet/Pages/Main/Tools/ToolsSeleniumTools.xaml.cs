using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet
{
    /// <summary>
    /// Logica di interazione per ToolsSeleniumTools.xaml
    /// </summary>
    public partial class ToolsSeleniumTools : Page
    {
        public ToolsSeleniumTools()
        {
            InitializeComponent();
        }

        
        private void killChromedriversButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("cmd.exe", "/C C:\\Windows\\System32\\taskkill.exe /F /IM chromedriver.exe /T");
        }

        
        private void killGeckodriversButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("cmd.exe", "/C C:\\Windows\\System32\\taskkill.exe /F /IM geckodriver.exe /T");
        }

        
        private void deleteChromeCacheFoldersButton_Click(object sender, RoutedEventArgs e)
        {
            string path = Environment.ExpandEnvironmentVariables("%userprofile%") + "\\AppData\\Local\\Temp";
            var directories = Directory.GetDirectories(path);
            foreach (var dir in directories)
            {
                if (dir.Contains("scopeddir") || dir.Contains("chromeurlfetcher") || dir.Contains("chromeBITS"))
                    try
                    {
                        Directory.Delete(dir, true);
                    }
                    catch { }
            }
        }

        
        private void deleteFirefoxCacheFoldersButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO
        }

        private void killChromesButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("cmd.exe", "/C C:\\Windows\\System32\\taskkill.exe /F /IM chrome.exe /T");
        }

        private void killFirefoxesButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("cmd.exe", "/C C:\\Windows\\System32\\taskkill.exe /F /IM firefox.exe /T");
        }
    }
}
