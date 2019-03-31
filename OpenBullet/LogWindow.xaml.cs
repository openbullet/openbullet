using RuriLib;
using System;
using System.Windows;
using System.Windows.Input;

namespace OpenBullet
{
    /// <summary>
    /// Logica di interazione per Log.xaml
    /// </summary>
    public partial class LogWindow : Window
    {
        public LogWindow()
        {
            InitializeComponent();
            DataContext = Globals.log;
            Closing += LogWindowClosing;
        }

        private void LogWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Globals.logWindow = null;
        }

        private void copyClick(object sender, RoutedEventArgs e)
        {
            var clipboardText = "";
            foreach (LogEntry selected in logListView.SelectedItems)
            {
                clipboardText += $"[{selected.LogTime}] ({selected.LogLevel}) {selected.LogComponent} - "+ selected.LogString + Environment.NewLine;
            }
            Clipboard.SetText(clipboardText);
        }

        private void copyAllButton_Click(object sender, RoutedEventArgs e)
        {
            var clipboardText = "";
            foreach (LogEntry selected in logListView.Items)
            {
                clipboardText += $"[{selected.LogTime}] ({selected.LogLevel}) {selected.LogComponent} - " + selected.LogString + Environment.NewLine;
            }
            Clipboard.SetText(clipboardText);
        }

        private void ListViewItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Globals.log.List.Clear();
            }
            catch { }
        }
        
        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            Globals.log.Refresh();
        }
    }
}
