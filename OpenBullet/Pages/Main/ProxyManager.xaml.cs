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
        public ProxyManagerViewModel vm = new ProxyManagerViewModel();
        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;
        private WorkerStatus Status = WorkerStatus.Idle;
        private CancellationTokenSource cts = new CancellationTokenSource();

        public ProxyManager()
        {
            InitializeComponent();
            DataContext = vm;

            vm.RefreshList();
            vm.UpdateProperties();
        }

        #region Start Button
        private void checkButton_Click(object sender, RoutedEventArgs e)
        {
            switch (Status)
            {
                case WorkerStatus.Idle:
                    Globals.LogInfo(Components.ProxyManager, "Disabling the UI and starting the checker");
                    checkButton.Content = "ABORT";
                    botsSlider.IsEnabled = false;
                    Status = WorkerStatus.Running;
#pragma warning disable CS4014 // Non è possibile attendere la chiamata, pertanto l'esecuzione del metodo corrente continuerà prima del completamento della chiamata
                    CheckProxiesAsync(vm.ProxyList, vm.BotsNumber, 200);
#pragma warning restore CS4014 // Non è possibile attendere la chiamata, pertanto l'esecuzione del metodo corrente continuerà prima del completamento della chiamata
                    break;

                case WorkerStatus.Running:
                    Globals.LogWarning(Components.ProxyManager, "Abort signal sent");
                    checkButton.Content = "HARD ABORT";
                    Status = WorkerStatus.Stopping;
                    cts.Cancel();
                    break;

                case WorkerStatus.Stopping:
                    Globals.LogWarning(Components.ProxyManager, "Hard abort signal sent");
                    checkButton.Content = "CHECK";
                    botsSlider.IsEnabled = true;
                    Status = WorkerStatus.Idle;
                    break;
            }
        }
        #endregion

        #region Check
        public async Task CheckProxiesAsync(IEnumerable<CProxy> proxies, int threads, int step)
        {
            var proxiesToCheck = vm.OnlyUntested ? proxies.ToList() : proxies.Where(p => p.Working == ProxyWorking.UNTESTED).ToList();

            // The semaphore will only allow {limit} elements at most running at the same time.
            using (var semaphore = new SemaphoreSlim(threads, threads))
            {
                cts = new CancellationTokenSource();

                for (int i = 0; i < proxiesToCheck.Count; i += step)
                {
                    var tasks = proxiesToCheck
                        .Skip(i)
                        .Take(Math.Min(proxiesToCheck.Count - i, step))
                        .Select(p => CheckProxyAsync(p, semaphore, cts.Token))
                        .ToArray();

                    try
                    {
                        await Task.WhenAny(Task.WhenAll(tasks), AsTask(cts.Token));
                        cts.Token.ThrowIfCancellationRequested();
                    }
                    catch
                    {
                        break;
                    }
                }

                App.Current.Dispatcher.Invoke(() =>
                {
                    Globals.LogInfo(Components.ProxyManager, "Check completed, re-enabling the UI");
                    checkButton.Content = "CHECK";
                    botsSlider.IsEnabled = true;
                    Status = WorkerStatus.Idle;
                });
            }
        }

        public static Task AsTask(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<object>();
            cancellationToken.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);
            return tcs.Task;
        }

        public async Task CheckProxyAsync(CProxy proxy, SemaphoreSlim semaphore, CancellationToken token)
        {
            await semaphore.WaitAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Status != WorkerStatus.Running) throw new OperationCanceledException();

                // We do it like this, otherwise if we make the requests async the ping measurement won't work
                await Task.Run(new Action(() =>
                {
                    CheckCountry(proxy);
                    CheckProxy(proxy);
                    App.Current.Dispatcher.Invoke(new Action(() => vm.UpdateProperties()));
                }));
            }
            catch (OperationCanceledException)
            {
                Globals.LogInfo(Components.ProxyManager, $"{proxy} - THROWING");
                throw;
            }
            finally
            {
                semaphore.Release();
            }
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

                    App.Current.Dispatcher.Invoke(new Action(() => proxy.Country = country.Replace("\"", "")));

                    using (var db = new LiteDatabase(Globals.dataBaseFile))
                    {
                        db.GetCollection<CProxy>("proxies").Update(proxy);
                    }

                    Globals.LogInfo(Components.ProxyManager, "Checked country for proxy '" + proxy.Proxy + "' with result '" + proxy.Country + "'");
                }

            }
            catch (Exception ex)
            {
                Globals.LogError(Components.ProxyManager, "Failted to check country for proxy '" + proxy.Proxy + $"' - {ex.Message}");
            }
        }

        private void CheckProxy(CProxy proxy)
        {
            var before = DateTime.Now;
            try
            {
                using (var request = new HttpRequest())
                {
                    request.Proxy = proxy.GetClient();
                    request.Proxy.ConnectTimeout = (int)vm.Timeout * 1000;
                    request.Proxy.ReadWriteTimeout = (int)vm.Timeout * 1000;
                    request.ConnectTimeout = (int)vm.Timeout * 1000;
                    request.KeepAliveTimeout = (int)vm.Timeout * 1000;
                    request.ReadWriteTimeout = (int)vm.Timeout * 1000;
                    var response = request.Get(vm.TestURL);
                    var source = response.ToString();

                    App.Current.Dispatcher.Invoke(new Action(() => proxy.Ping = (int)(DateTime.Now - before).TotalMilliseconds));

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
        #endregion

        private void ProxyListViewDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (var file in files.Where(x => x.EndsWith(".txt")).ToArray())
                {
                    try
                    {
                        var lines = File.ReadAllText(file).Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();

                        if ((file.ToLower()).Contains("http"))
                        {
                            AddProxies(ProxyType.Http, lines);
                        }
                        else if ((file.ToLower()).Contains("socks4a"))
                        {
                            AddProxies(ProxyType.Socks4a, lines);
                        }
                        else if ((file.ToLower()).Contains("socks4"))
                        {
                            AddProxies(ProxyType.Socks4, lines);
                        }
                        else if ((file.ToLower()).Contains("socks5"))
                        {
                            AddProxies(ProxyType.Socks5, lines);
                        }
                        else
                        {
                            Globals.LogError(Components.ProxyManager, "Failed to parse proxies type from file name, defaulting to HTTP");
                            AddProxies(ProxyType.Http, lines);
                        }
                    }
                    catch { }
                }
            }
        }
        public void AddProxies(ProxyType type, List<string> lines)
        {
            Globals.LogInfo(Components.ProxyManager, $"Adding {lines.Count} {type} proxies to the database");

            // Check if they're valid
            var proxies = new List<CProxy>();

            foreach (var p in lines.Where(p => !string.IsNullOrEmpty(p)).Distinct().ToList())
            {
                try
                {
                    CProxy proxy = new CProxy(p, type);
                    if (!proxy.IsNumeric || proxy.IsValidNumeric)
                    {
                        vm.ProxyList.Add(proxy);
                        proxies.Add(proxy);
                    }
                }
                catch { }
            }

            using (var db = new LiteDatabase(Globals.dataBaseFile))
            {
                db.GetCollection<CProxy>("proxies").InsertBulk(proxies);
            }

            // Refresh
            vm.UpdateProperties();
        }

        #region GUI Controls
        private void botsSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            vm.BotsNumber = (int)e.NewValue;
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

        private void importButton_Click(object sender, RoutedEventArgs e)
        {
            (new MainDialog(new DialogAddProxies(this), "Import Proxies")).ShowDialog();
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

        private void DeleteUntestedButton_Click(object sender, RoutedEventArgs e)
        {
            Globals.LogInfo(Components.ProxyManager, "Deleting all untested proxies");

            using (var db = new LiteDatabase(Globals.dataBaseFile))
            {
                db.GetCollection<CProxy>("proxies").Delete(p => p.Working == ProxyWorking.UNTESTED);
                var list = vm.ProxyList.Where(p => p.Working == ProxyWorking.UNTESTED);
                while (list.Count() > 0)
                {
                    vm.ProxyList.Remove(list.First());
                }
            }

            vm.UpdateProperties();
        }
        #endregion

        #region ListView
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

        private void ListViewItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
        #endregion
    }
}
