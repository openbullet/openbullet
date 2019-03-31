using Extreme.Net;
using LiteDB;
using Microsoft.Win32;
using OpenBullet.ViewModels;
using RuriLib.Models;
using RuriLib.Runner;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace OpenBullet
{
    /// <summary>
    /// Logica di interazione per ProxyManager.xaml
    /// </summary>
    public partial class ProxyManager : Page
    {
        AbortableBackgroundWorker bw = new AbortableBackgroundWorker();
        public ProxyManagerViewModel vm = new ProxyManagerViewModel();
        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;
        bool stop = false;

        public ProxyManager()
        {
            InitializeComponent();
            DataContext = vm;

            bw.DoWork += new DoWorkEventHandler(Check);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(CheckComplete);

            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;
            bw.Status = WorkerStatus.Idle;

            vm.RefreshList();
            vm.UpdateProperties();
        }

        
        private void Check(object sender, DoWorkEventArgs e)
        {
            stop = false;
            ThreadPool.SetMinThreads(vm.BotsNumber*2, vm.BotsNumber*2);
            Globals.LogInfo(Components.ProxyManager, "Set the minimum threads");
            //ThreadPool.SetMaxThreads(1000, 1000);
            Parallel.ForEach(vm.OnlyUntested ? vm.ProxyList.Where(p => p.Working == ProxyWorking.UNTESTED) : vm.ProxyList,
                new ParallelOptions { MaxDegreeOfParallelism = vm.BotsNumber }, (proxy, state) =>
            {
                if (stop) { Globals.LogWarning(Components.ProxyManager, "Abort signal received, breaking the state"); state.Break(); }
                CheckCountry(proxy);
                CheckProxy(proxy);
                App.Current.Dispatcher.Invoke(new Action(() => vm.UpdateProperties()));
            });
        }

        
        private void CheckCountry(CProxy proxy)
        {
            try
            {
                using (var request = new HttpRequest())
                {
                    request.ConnectTimeout = (int)vm.Timeout;
                    var response = request.Get("http://ip-api.com/csv/" + proxy.Host);
                    var csv = response.ToString();
                    var split = csv.Split(',');

                    var country = "Unknown";

                    if (split[0] == "success")
                        country = split[1];

                    proxy.Country = country.Replace("\"", "");

                    using (var db = new LiteDatabase(Globals.dataBaseFile))
                    {
                        db.GetCollection<CProxy>("proxies").Update(proxy);
                    }

                    Globals.LogInfo(Components.ProxyManager, "Checked country for proxy '" + proxy.Proxy + "' with result '" + proxy.Country + "'");
                }
                
            }
            catch (Exception ex) { Globals.LogError(Components.ProxyManager, "Failted to check country for proxy '" + proxy.Proxy + $"' - {ex.Message}"); }
        }

        private void CheckProxy(CProxy proxy)
        {
            var before = DateTime.Now;
            try
            {
                using (var request = new HttpRequest())
                {
                    request.Proxy = proxy.GetClient();
                    request.ConnectTimeout = (int)vm.Timeout;
                    var response = request.Get(vm.TestURL);
                    var source = response.ToString();
                    
                    App.Current.Dispatcher.Invoke(new Action(() => proxy.Ping = (DateTime.Now - before).Milliseconds));

                    App.Current.Dispatcher.Invoke(new Action(() => proxy.Working = source.Contains(vm.SuccessKey) ? ProxyWorking.YES : ProxyWorking.NO));

                    Globals.LogInfo(Components.ProxyManager, "Proxy '" + proxy.Proxy + $"' responded in {proxy.Ping} ms");
                }
            }
            catch (Exception ex)
            {
                Globals.LogInfo(Components.ProxyManager, "Proxy '" + proxy.Proxy + $"' failed to respond - {ex.Message}");
                App.Current.Dispatcher.Invoke(new Action(() => proxy.Working = ProxyWorking.NO));
            }

            using (var db = new LiteDatabase(Globals.dataBaseFile))
            {
                db.GetCollection<CProxy>("proxies").Update(proxy);
            }
        }

        
        private void CheckComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            Globals.LogInfo(Components.ProxyManager, "Check completed, re-enabling the UI");
            checkButton.Content = "CHECK";
            botsSlider.IsEnabled = true;
            bw.Status = WorkerStatus.Idle;
        }

        private void botsSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            vm.BotsNumber = (int)e.NewValue;
        }

        private void checkButton_Click(object sender, RoutedEventArgs e)
        {
            switch (bw.Status)
            {
                case WorkerStatus.Idle:
                    stop = false;
                    Globals.LogInfo(Components.ProxyManager, "Disabling the UI and starting the checker");
                    checkButton.Content = "ABORT";
                    botsSlider.IsEnabled = false;
                    bw.RunWorkerAsync();
                    bw.Status = WorkerStatus.Running;
                    break;

                case WorkerStatus.Running:
                    stop = true;
                    Globals.LogWarning(Components.ProxyManager, "Abort signal sent");
                    checkButton.Content = "HARD ABORT";
                    bw.Status = WorkerStatus.Stopping;
                    break;

                case WorkerStatus.Stopping:
                    Globals.LogWarning(Components.ProxyManager, "Hard abort signal sent");
                    bw.CancelAsync();
                    break;
            }
        }

        private void importButton_Click(object sender, RoutedEventArgs e)
        {
            (new MainDialog(new DialogAddProxies(this), "Import Proxies")).ShowDialog();
        }

        public void AddProxies(string fileName, ProxyType type, List<string> lines)
        {
            List<string> fromFile = new List<string>();
            List<string> fromBox = new List<string>();

            // Load proxies from file
            if (fileName != "")
            {
                Globals.LogInfo(Components.ProxyManager, $"Trying to load from file {fileName}");
                fromFile.AddRange(File.ReadAllLines(fileName).ToList());
            }
            else { Globals.LogInfo(Components.ProxyManager, "No file specified, skipping the import from file"); }

            // Load proxies from textbox lines
            fromBox.AddRange(lines);

            Globals.LogInfo(Components.ProxyManager, $"Adding {fromFile.Count + fromBox.Count} proxies to the database");

            // Check if they're valid
            using (var db = new LiteDatabase(Globals.dataBaseFile))
            {
                foreach (var p in fromFile.Where(p => !string.IsNullOrEmpty(p)).Distinct().ToList())
                {
                    try
                    {
                        CProxy proxy = new CProxy(p, type);
                        if (!proxy.IsNumeric || proxy.IsValidNumeric)
                        {
                            vm.ProxyList.Add(proxy);
                            db.GetCollection<CProxy>("proxies").Insert(proxy);
                        }
                    }
                    catch { }
                }

                foreach (var p in fromBox.Where(p => !string.IsNullOrEmpty(p)).Distinct().ToList())
                {
                    try
                    {
                        CProxy proxy = new CProxy();
                        proxy.Parse(p);
                        if (!proxy.IsNumeric || proxy.IsValidNumeric)
                        {
                            vm.ProxyList.Add(proxy);
                            db.GetCollection<CProxy>("proxies").Insert(proxy);
                        }
                    }
                    catch { }
                }
            }

            // Refresh
            vm.UpdateProperties();
        }

        
        private void exportButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Text File |*.txt";
            sfd.Title = "Export proxies";
            sfd.ShowDialog();

            if (sfd.FileName != "")
            {
                if (proxiesListView.Items.Count > 0)
                {
                    Globals.LogInfo(Components.ProxyManager, $"Exporting {proxiesListView.Items.Count} proxies");
                    List<string> toExport = new List<string>();

                    foreach (CProxy p in proxiesListView.SelectedItems)
                        toExport.Add(p.Proxy);

                    File.WriteAllLines(sfd.FileName, toExport);
                }
                else
                {
                    System.Windows.MessageBox.Show("No proxies selected!");
                    Globals.LogWarning(Components.ProxyManager, "No proxies selected");
                }
            }
        }

        
        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            Globals.LogInfo(Components.ProxyManager, $"Deleting {proxiesListView.SelectedItems.Count} proxies");
            List<CProxy> toDelete = new List<CProxy>();
            foreach (CProxy proxy in proxiesListView.SelectedItems)
                toDelete.Add(proxy);
            DeleteProxies(toDelete);
            Globals.LogInfo(Components.ProxyManager, "Proxies deleted successfully");
        }

        
        private void DeleteProxies(List<CProxy> proxies)
        {
            Globals.LogInfo(Components.ProxyManager, "Deleting selected proxies");

            using (var db = new LiteDatabase(Globals.dataBaseFile))
            {
                var list = proxiesListView.SelectedItems.Cast<CProxy>();
                while (list.Count() > 0)
                {
                    db.GetCollection<CProxy>("proxies").Delete(list.First().Id);
                    vm.ProxyList.Remove(list.First());
                }
            }

            vm.UpdateProperties();
        }

        
        private void deleteAllButton_Click(object sender, RoutedEventArgs e)
        {
            Globals.LogWarning(Components.ProxyManager, "Purging all proxies");
            using (var db = new LiteDatabase(Globals.dataBaseFile))
            {
                db.DropCollection("proxies");
            }

            vm.ProxyList.Clear();

            vm.UpdateProperties();
        }

        
        private void deleteNotWorkingButton_Click(object sender, RoutedEventArgs e)
        {
            Globals.LogInfo(Components.ProxyManager, "Deleting all non working proxies");

            using (var db = new LiteDatabase(Globals.dataBaseFile))
            {
                db.GetCollection<CProxy>("proxies").Delete(p => p.Working == ProxyWorking.NO);
                var list = vm.ProxyList.Where(p => p.Working == ProxyWorking.NO);
                while (list.Count() > 0)
                {
                    vm.ProxyList.Remove(list.First());
                }
            }

            vm.UpdateProperties();
        }

        private void listViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            string sortBy = column.Tag.ToString();
            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                proxiesListView.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            proxiesListView.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }

        private void copySelectedProxies_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var toCopy = "";
                foreach (CProxy proxy in proxiesListView.SelectedItems)
                    toCopy += proxy.Proxy + Environment.NewLine;

                Clipboard.SetText(toCopy);
                Globals.LogInfo(Components.ProxyManager, $"Copied {proxiesListView.SelectedItems.Count} proxies");
            }
            catch (Exception ex) { Globals.LogError(Components.ProxyManager, $"Failed to copy proxies - {ex.Message}"); }
        }

        private void ListViewItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void deleteDuplicatesButton_Click(object sender, RoutedEventArgs e)
        {
            var dupeList = vm.ProxyList
                .GroupBy(p => p.Proxy)
                .Where(grp => grp.Count() > 1)
                .Select(grp => grp.First())
                .ToList();

            Globals.LogInfo(Components.ProxyManager, $"Removing {vm.ProxyList.Count - dupeList.Count} duplicate proxies");

            using (var db = new LiteDatabase(Globals.dataBaseFile))
            {
                for (int i = 0; i < dupeList.Count; i++)
                {
                    var p = dupeList[i];
                    vm.ProxyList.Remove(p);
                    db.GetCollection<CProxy>("proxies").Delete(p.Id);
                }
            }

            vm.UpdateProperties();
        }
    }
}
