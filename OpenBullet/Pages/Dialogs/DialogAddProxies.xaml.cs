using Extreme.Net;
using Microsoft.Win32;
using System;
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
            if(Caller.GetType() == typeof(ProxyManager))
            {
                ((ProxyManager)Caller).AddProxies(locationTextbox.Text,
                    (ProxyType)Enum.Parse(typeof(ProxyType), proxyTypeCombobox.Text),
                    proxiesBox.Text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None).ToList());
            }
            ((MainDialog)Parent).Close();
        }
    }
}
