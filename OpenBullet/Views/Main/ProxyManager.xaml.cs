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

namespace OpenBullet.Views.Main
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

        private IEnumerable<CProxy> Selected => proxiesListView.SelectedItems.Cast<CProxy>();

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
                    CheckProxiesAsync(vm.ProxiesCollection, vm.BotsAmount, 200);
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
                    var response = request.Get(vm.TestSite);
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

        // TODO: Refactor this function, it shouldn't belong in a view!
        public void AddProxies(IEnumerable<string> raw, ProxyType defaultType = ProxyType.Http, string defaultUsername = "", string defaultPassword = "")
        {
            Globals.LogInfo(Components.ProxyManager, $"Adding {raw.Count()} {defaultType} proxies to the database");

            // Check if they're valid
            var proxies = new List<CProxy>();

            foreach (var p in raw.Where(p => !string.IsNullOrEmpty(p)).Distinct().ToList())
            {
                try
                {
                    CProxy proxy = new CProxy().Parse(p, defaultType, defaultUsername, defaultPassword);
                    if (!proxy.IsNumeric || proxy.IsValidNumeric)
                    {
                        proxies.Add(proxy);
                    }
                }
                catch { }
            }

            vm.AddRange(proxies);

            // Refresh
            vm.UpdateProperties();
        }

        private void botsSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            vm.BotsAmount = (int)e.NewValue;
        }

        #region Buttons
        private void exportButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Text File |*.txt";
            sfd.Title = "Export proxies";
            sfd.ShowDialog();

            if (sfd.FileName != "")
            {
                if (Selected.Count() > 0)
                {
                    Globals.LogInfo(Components.ProxyManager, $"Exporting {proxiesListView.Items.Count} proxies");
                    Selected.SaveToFile(sfd.FileName, p => p.Proxy);
                }
                else
                {
                    MessageBox.Show("No proxies selected!");
                    Globals.LogWarning(Components.ProxyManager, "No proxies selected");
                }
            }
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            Globals.LogInfo(Components.ProxyManager, $"Deleting {proxiesListView.SelectedItems.Count} proxies");
            vm.Remove(Selected);
            vm.UpdateProperties();
            Globals.LogInfo(Components.ProxyManager, "Proxies deleted successfully");
        }

        private void deleteAllButton_Click(object sender, RoutedEventArgs e)
        {
            Globals.LogWarning(Components.ProxyManager, "Purging all proxies");
            vm.RemoveAll();
            vm.UpdateProperties();
        }

        private void deleteNotWorkingButton_Click(object sender, RoutedEventArgs e)
        {
            Globals.LogInfo(Components.ProxyManager, "Deleting all non working proxies");

            vm.RemoveNotWorking();
            vm.UpdateProperties();
        }

        private void importButton_Click(object sender, RoutedEventArgs e)
        {
            (new MainDialog(new DialogAddProxies(this), "Import Proxies")).ShowDialog();
        }

        private void deleteDuplicatesButton_Click(object sender, RoutedEventArgs e)
        {
            Globals.LogInfo(Components.ProxyManager, "Deleting duplicate proxies");

            vm.RemoveDuplicates();
            vm.UpdateProperties();
        }

        private void DeleteUntestedButton_Click(object sender, RoutedEventArgs e)
        {
            Globals.LogInfo(Components.ProxyManager, "Deleting all untested proxies");

            vm.RemoveUntested();
            vm.UpdateProperties();
        }
        #endregion

        #region ListView
        private void ProxyListViewDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (var file in files.Where(x => x.EndsWith(".txt")).ToArray())
                {
                    try
                    {
                        var lines = File.ReadAllLines(file);

                        if ((file.ToLower()).Contains("http"))
                        {
                            AddProxies(lines, ProxyType.Http);
                        }
                        else if ((file.ToLower()).Contains("socks4a"))
                        {
                            AddProxies(lines, ProxyType.Socks4a);
                        }
                        else if ((file.ToLower()).Contains("socks4"))
                        {
                            AddProxies(lines, ProxyType.Socks4);
                        }
                        else if ((file.ToLower()).Contains("socks5"))
                        {
                            AddProxies(lines, ProxyType.Socks5);
                        }
                        else
                        {
                            Globals.LogError(Components.ProxyManager, "Failed to parse proxies type from file name, defaulting to HTTP");
                            AddProxies(lines);
                        }
                    }
                    catch (Exception ex)
                    {
                        Globals.LogError(Components.ProxyManager, $"Failed to open file {file} - {ex.Message}");
                    }
                }
            }
        }

        private void copySelectedProxies_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Selected.CopyToClipboard(p => p.Proxy);
                Globals.LogInfo(Components.ProxyManager, $"Copied {Selected.Count()} proxies");
            }
            catch (Exception ex)
            {
                Globals.LogError(Components.ProxyManager, $"Failed to copy proxies - {ex.Message}");
            }
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

        private void ListViewItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
        #endregion
    }
}
