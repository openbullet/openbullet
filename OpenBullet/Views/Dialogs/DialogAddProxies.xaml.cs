using Extreme.Net;
using Microsoft.Win32;
using OpenBullet.Views;
using OpenBullet.Views.Main;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpenBullet
{
    /// <summary>
    /// Logica di interazione per DialogAddProxies.xaml
    /// </summary>
    public partial class DialogAddProxies : Page
    {
        public object Caller { get; set; }

        public DialogAddProxies(object caller)
        {
            InitializeComponent();
            Caller = caller;
            foreach (string i in Enum.GetNames(typeof(ProxyType)))
                if(i != "Chain") proxyTypeCombobox.Items.Add(i);
            proxyTypeCombobox.SelectedIndex = 0;
        }

        
        private void loadProxiesButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Proxy files | *.txt";
            ofd.FilterIndex = 1;
            ofd.ShowDialog();
            locationTextbox.Text = ofd.FileName;
        }

        
        private void acceptButton_Click(object sender, RoutedEventArgs e)
        {
            var fileName = locationTextbox.Text;
            List<string> lines = new List<string>();

            try
            {
                switch (modeTabControl.SelectedIndex)
                {
                    // File
                    case 0:
                        if (fileName != "")
                        {
                            OB.Logger.LogInfo(Components.ProxyManager, $"Trying to load from file {fileName}");
                            lines.AddRange(File.ReadAllLines(fileName).ToList());
                        }
                        else
                        {
                            OB.Logger.LogError(Components.ProxyManager, "No file specified!", true);
                            return;
                        }
                        break;

                    case 1:
                        if (proxiesBox.Text != "")
                        {
                            lines.AddRange(proxiesBox.Text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None));
                        }
                        else
                        {
                            OB.Logger.LogError(Components.ProxyManager, "The box is empty!", true);
                            return;
                        }
                        break;

                    case 2:
                        if (urlTextbox.Text != "")
                        {
                            HttpRequest request = new HttpRequest();
                            var response = request.Get(urlTextbox.Text).ToString();
                            lines.AddRange(response.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None));
                        }
                        else
                        {
                            OB.Logger.LogError(Components.ProxyManager, "No URL specified!", true);
                            return;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                OB.Logger.LogError(Components.ProxyManager, $"There was an error: {ex.Message}");
                return;
            }

            if(Caller.GetType() == typeof(ProxyManager))
            {
                ((ProxyManager)Caller).AddProxies(lines, (ProxyType)Enum.Parse(typeof(ProxyType), proxyTypeCombobox.Text), usernameTextbox.Text, passwordTextbox.Text);
            }
            ((MainDialog)Parent).Close();
        }

        private void FileMode_MouseDown(object sender, MouseButtonEventArgs e)
        {
            fileMode.Foreground = Utils.GetBrush("ForegroundMenuSelected");
            pasteMode.Foreground = Utils.GetBrush("ForegroundMain");
            apiMode.Foreground = Utils.GetBrush("ForegroundMain");
            modeTabControl.SelectedIndex = 0;
        }

        private void PasteMode_MouseDown(object sender, MouseButtonEventArgs e)
        {
            fileMode.Foreground = Utils.GetBrush("ForegroundMain");
            pasteMode.Foreground = Utils.GetBrush("ForegroundMenuSelected");
            apiMode.Foreground = Utils.GetBrush("ForegroundMain");
            modeTabControl.SelectedIndex = 1;
        }

        private void ApiMode_MouseDown(object sender, MouseButtonEventArgs e)
        {
            fileMode.Foreground = Utils.GetBrush("ForegroundMain");
            pasteMode.Foreground = Utils.GetBrush("ForegroundMain");
            apiMode.Foreground = Utils.GetBrush("ForegroundMenuSelected");
            modeTabControl.SelectedIndex = 2;
        }
    }
}
