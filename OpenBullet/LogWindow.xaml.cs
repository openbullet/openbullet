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
            DataContext = OB.Logger;
            Closing += LogWindowClosing;
        }

        private void LogWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            OB.LogWindow = null;
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
                OB.Logger.EntriesCollection.Clear();
            }
            catch { }
        }
        
        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            OB.Logger.Refresh();
        }
    }
}
