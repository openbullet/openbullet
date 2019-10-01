using Extreme.Net;
using Microsoft.Win32;
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
                            Globals.LogInfo(Components.ProxyManager, $"Trying to load from file {fileName}");
                            lines.AddRange(File.ReadAllLines(fileName).ToList());
                        }
                        else
                        {
                            Globals.LogError(Components.ProxyManager, "No file specified!", true);
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
                            Globals.LogError(Components.ProxyManager, "The box is empty!", true);
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
                            Globals.LogError(Components.ProxyManager, "No URL specified!", true);
                            return;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Globals.LogError(Components.ProxyManager, $"There was an error: {ex.Message}");
                return;
            }

            if(Caller.GetType() == typeof(ProxyManager))
            {
                ((ProxyManager)Caller).AddProxies((ProxyType)Enum.Parse(typeof(ProxyType), proxyTypeCombobox.Text), lines);
            }
            ((MainDialog)Parent).Close();
        }

        private void FileMode_MouseDown(object sender, MouseButtonEventArgs e)
        {
            fileMode.Foreground = Globals.GetBrush("ForegroundMenuSelected");
            pasteMode.Foreground = Globals.GetBrush("ForegroundMain");
            apiMode.Foreground = Globals.GetBrush("ForegroundMain");
            modeTabControl.SelectedIndex = 0;
        }

        private void PasteMode_MouseDown(object sender, MouseButtonEventArgs e)
        {
            fileMode.Foreground = Globals.GetBrush("ForegroundMain");
            pasteMode.Foreground = Globals.GetBrush("ForegroundMenuSelected");
            apiMode.Foreground = Globals.GetBrush("ForegroundMain");
            modeTabControl.SelectedIndex = 1;
        }

        private void ApiMode_MouseDown(object sender, MouseButtonEventArgs e)
        {
            fileMode.Foreground = Globals.GetBrush("ForegroundMain");
            pasteMode.Foreground = Globals.GetBrush("ForegroundMain");
            apiMode.Foreground = Globals.GetBrush("ForegroundMenuSelected");
            modeTabControl.SelectedIndex = 2;
        }
    }
}
