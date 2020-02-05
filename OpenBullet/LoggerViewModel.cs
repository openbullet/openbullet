using RuriLib;
using RuriLib.Interfaces;
using RuriLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace OpenBullet
{
    public enum Components
    {
        Main,
        RunnerManager,
        Runner,
        ProxyManager,
        WordlistManager,
        HitsDB,
        ConfigManager,
        Stacker,
        OtherOptions,
        Settings,
        ListGenerator,
        SeleniumTools,
        Database,
        About,
        Unknown
    }

    public class LoggerViewModel : ViewModelBase, ILogger
    {
        public ObservableCollection<LogEntry> EntriesCollection { get; set; }

        public IEnumerable<LogEntry> Entries => EntriesCollection;

        public bool Enabled
        {
            get
            {
                try
                {
                    // The settings might be null
                    return OB.OBSettings.General.EnableLogging;
                }
                catch
                {
                    return false;
                }
            }
        }

        public int BufferSize
        {
            get
            {
                try
                {
                    // The settings might be null
                    return OB.OBSettings.General.LogBufferSize;
                }
                catch
                {
                    return 0;
                }
            }
        }

        public LoggerViewModel()
        {
            EntriesCollection = new ObservableCollection<LogEntry>();

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(EntriesCollection);
            view.Filter = ErrorFilter;
        }

        public void Refresh()
        {
            try
            {
                CollectionViewSource.GetDefaultView(EntriesCollection).Refresh();
            }
            catch { }
        }

        #region Filters
        private bool onlyErrors = false;
        public bool OnlyErrors { get { return onlyErrors; } set { onlyErrors = value; OnPropertyChanged(); Refresh(); } }
        
        private string searchString = "";
        public string SearchString { get { return searchString; } set { searchString = value; OnPropertyChanged(); } }

        private bool ErrorFilter(object item)
        {
            // If search box not empty, filter out all the stuff that's not needed
            if (SearchString != string.Empty)
            {
                if (!(item as LogEntry).LogString.ToLower().Contains(SearchString.ToLower()))
                    return false;
            }

            if (!OB.Logger.OnlyErrors)
                return true;

            else
                return ((item as LogEntry).LogLevel == LogLevel.Error);
        }
        #endregion

        public void Log(string message, LogLevel level, bool prompt = false, int timeout = 0)
        {
            Log(Components.Unknown, level, message, prompt, timeout);
        }

        public void Log(Components component, LogLevel level, string message, bool prompt = false, int timeout = 0)
        {
            if (prompt)
            {
                if (timeout == 0)
                {
                    MessageBox.Show(message, level.ToString());
                }
                else
                {
                    var w = new System.Windows.Forms.Form() { Size = new System.Drawing.Size(0, 0) };
                    Task.Delay(TimeSpan.FromSeconds(timeout))
                        .ContinueWith((t) => w.Close(), TaskScheduler.FromCurrentSynchronizationContext());

                    System.Windows.Forms.MessageBox.Show(w, message, level.ToString());
                }
            }

            if (!Enabled)
            {
                return;
            }

            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                var entry = new LogEntry(component.ToString(), message, level);
                InsertEntry(entry);
                LogToFile(entry);
            }));
        }

        public void LogInfo(Components component, string message, bool prompt = false, int timeout = 0)
        {
            Log(component, LogLevel.Info, message, prompt, timeout);
        }

        public void LogWarning(Components component, string message, bool prompt = false, int timeout = 0)
        {
            Log(component, LogLevel.Warning, message, prompt, timeout);
        }

        public void LogError(Components component, string message, bool prompt = false, int timeout = 0)
        {
            Log(component, LogLevel.Error, message, prompt, timeout);
        }

        private void InsertEntry(LogEntry entry)
        {
            try
            {
                EntriesCollection.Insert(0, entry);

                var count = EntriesCollection.Count;
                if (count > BufferSize)
                {
                    EntriesCollection.RemoveAt(count - 1);
                }
            }
            catch { }
        }

        private static void LogToFile(LogEntry entry)
        {
            try
            {
                if (OB.OBSettings.General.LogToFile)
                {
                    File.AppendAllText(OB.logFile, $"[{entry.LogTime}] ({entry.LogLevel}) {entry.LogComponent} - " + entry.LogString + Environment.NewLine);
                }
            }
            catch { }
        }
    }
}
