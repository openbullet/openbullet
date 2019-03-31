using RuriLib;
using RuriLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpenBullet
{
    /// <summary>
    /// Logica di interazione per DialogShowLog.xaml
    /// </summary>
    public partial class DialogShowLog : Page
    {
        public FullLogViewModel vm = new FullLogViewModel();

        public DialogShowLog(List<LogEntry> log)
        {
            InitializeComponent();
            DataContext = vm;

            // Style the logRTB
            logRTB.Font = new System.Drawing.Font("Consolas", 10);
            logRTB.BackColor = System.Drawing.Color.FromArgb(22, 22, 22);

            foreach (var entry in log)
            {
                logRTB.AppendText(entry.LogString + Environment.NewLine, entry.LogColor);
            }
        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            Globals.LogInfo(Components.Stacker, $"Seaching for {vm.SearchString}");

            // Reset all highlights
            logRTB.SelectAll();
            logRTB.SelectionBackColor = System.Drawing.Color.FromArgb(22, 22, 22);
            logRTB.DeselectAll();

            // Check for empty search
            if (vm.SearchString == string.Empty)
                return;

            int sstart = logRTB.SelectionStart, startIndex = 0, index;
            vm.Indexes.Clear();

            while ((index = logRTB.Text.IndexOf(vm.SearchString, startIndex, StringComparison.InvariantCultureIgnoreCase)) != -1)
            {
                logRTB.Select(index, vm.SearchString.Length);
                logRTB.SelectionColor = System.Drawing.Color.White;
                logRTB.SelectionBackColor = System.Drawing.Color.Navy;

                startIndex = index + vm.SearchString.Length;
                vm.Indexes.Add(startIndex);
                if (vm.Indexes.Count == 1) logRTB.ScrollToCaret();
            }

            vm.UpdateTotalSearchMatches();

            // Reset the selection
            logRTB.SelectionStart = sstart;
            logRTB.SelectionLength = 0;
            logRTB.SelectionColor = System.Drawing.Color.Black;

            Globals.LogInfo(Components.Stacker, $"Found {vm.Indexes.Count} matches", true);

            if (vm.Indexes.Count > 0)
                vm.CurrentSearchMatch = 1;
        }

        private void previousMatchButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (vm.CurrentSearchMatch == 1 || vm.TotalSearchMatches == 0)
                return;

            vm.CurrentSearchMatch--;
            logRTB.DeselectAll();
            logRTB.Select(vm.Indexes[vm.CurrentSearchMatch - 1], 0);
            logRTB.ScrollToCaret();
        }

        private void nextMatchButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (vm.CurrentSearchMatch == vm.Indexes.Count || vm.TotalSearchMatches == 0)
                return;

            vm.CurrentSearchMatch++;
            logRTB.DeselectAll();
            logRTB.Select(vm.Indexes[vm.CurrentSearchMatch - 1], 0);
            logRTB.ScrollToCaret();
        }
    }

    public class FullLogViewModel : ViewModelBase
    {
        // Search
        private string searchString = "";
        public string SearchString { get { return searchString; } set { searchString = value; OnPropertyChanged("SearchString"); OnPropertyChanged("SearchProgress"); } }

        private List<int> indexes = new List<int>();
        public List<int> Indexes { get { return indexes; } set { indexes = value; OnPropertyChanged("TotalSearchMatches"); OnPropertyChanged("CurrentSearchMatch"); } }
        public int TotalSearchMatches { get { return Indexes.Count; } }

        public void UpdateTotalSearchMatches()
        {
            OnPropertyChanged("TotalSearchMatches");
        }

        private int currentSearchMatch = 0;
        public int CurrentSearchMatch { get { return currentSearchMatch; } set { currentSearchMatch = value; OnPropertyChanged("CurrentSearchMatch"); } }
    }
}
