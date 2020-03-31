using Extreme.Net;
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
        private ProxyManagerViewModel vm = null;
        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;
        private WorkerStatus Status = WorkerStatus.Idle;
        private CancellationTokenSource cts = new CancellationTokenSource();

        private IEnumerable<CProxy> Selected => proxiesListView.SelectedItems.Cast<CProxy>();

        public ProxyManager()
        {
            vm = OB.ProxyManager;
            DataContext = vm;

            InitializeComponent();
            botsSlider.Maximum = ProxyManagerViewModel.maximumBots;
            vm.RefreshList();
            vm.UpdateProperties();
        }

        #region Start Button
        private async void checkButton_Click(object sender, RoutedEventArgs e)
        {
            switch (Status)
            {
                case WorkerStatus.Idle:
                    OB.Logger.LogInfo(Components.ProxyManager, "Disabling the UI and starting the checker");
                    checkButton.Content = "ABORT";
                    botsSlider.IsEnabled = false;
                    Status = WorkerStatus.Running;

                    var items = vm.OnlyUntested ? vm.Proxies.Where(p => p.Working == ProxyWorking.UNTESTED) : vm.Proxies;

                    // Setup the progress bar
                    progressBar.Value = 0;

                    // Start checking
                    cts = new CancellationTokenSource();

                    try
                    {
                        await vm.CheckAllAsync(items, cts.Token,
                        new Action<CheckResult<ProxyResult>>(check =>
                        {
                            var result = check.result;
                            var proxy = result.proxy;

                            proxy.LastChecked = DateTime.Now;

                            if (check.success)
                            {
                                // Set all the changed proxy fields
                                proxy.Working = result.working ? ProxyWorking.YES : ProxyWorking.NO;
                                proxy.Ping = result.ping;
                                proxy.Country = result.country;

                                var infoLog = $"[{DateTime.Now.ToLongTimeString()}] Check for proxy {proxy.Proxy} succeeded in {result.ping} milliseconds.";
                                OB.Logger.LogInfo(Components.ProxyManager, infoLog);
                            }
                            else
                            {
                                proxy.Working = ProxyWorking.NO;
                                proxy.Ping = 0;

                                var errorLog = $"[{DateTime.Now.ToLongTimeString()}] Check for proxy {proxy.Proxy} failed with error: {check.error}";
                                OB.Logger.LogError(Components.ProxyManager, errorLog);
                            }

                            // Update the proxy in the database
                            vm.Update(proxy);
                        }),
                        new Progress<float>(progress =>
                        {
                            progressBar.Value = progress;
                            vm.UpdateProperties();
                        }));
                    }
                    catch
                    {
                        OB.Logger.LogWarning(Components.ProxyManager, "Abort signal received");
                    }
                    // Restore the GUI status
                    finally
                    {
                        checkButton.Content = "CHECK";
                        botsSlider.IsEnabled = true;
                        Status = WorkerStatus.Idle;
                    }
                    break;

                case WorkerStatus.Running:
                    cts.Cancel();
                    break;
            }
        }
        #endregion

        // TODO: Refactor this function, it shouldn't belong in a view!
        public void AddProxies(IEnumerable<string> raw, ProxyType defaultType = ProxyType.Http, string defaultUsername = "", string defaultPassword = "")
        {
            OB.Logger.LogInfo(Components.ProxyManager, $"Adding {raw.Count()} {defaultType} proxies to the database");

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

            if (sfd.FileName != string.Empty)
            {
                if (Selected.Count() > 0)
                {
                    OB.Logger.LogInfo(Components.ProxyManager, $"Exporting {proxiesListView.Items.Count} proxies");
                    Selected.SaveToFile(sfd.FileName, p => p.Proxy);
                }
                else
                {
                    MessageBox.Show("No proxies selected!");
                    OB.Logger.LogWarning(Components.ProxyManager, "No proxies selected");
                }
            }
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            OB.Logger.LogInfo(Components.ProxyManager, $"Deleting {proxiesListView.SelectedItems.Count} proxies");
            vm.Remove(Selected);
            vm.UpdateProperties();
            OB.Logger.LogInfo(Components.ProxyManager, "Proxies deleted successfully");
        }

        private void deleteAllButton_Click(object sender, RoutedEventArgs e)
        {
            OB.Logger.LogWarning(Components.ProxyManager, "Purging all proxies");
            vm.RemoveAll();
            vm.UpdateProperties();
        }

        private void deleteNotWorkingButton_Click(object sender, RoutedEventArgs e)
        {
            OB.Logger.LogInfo(Components.ProxyManager, "Deleting all non working proxies");

            vm.RemoveNotWorking();
            vm.UpdateProperties();
        }

        private void importButton_Click(object sender, RoutedEventArgs e)
        {
            (new MainDialog(new DialogAddProxies(this), "Import Proxies")).ShowDialog();
        }

        private void deleteDuplicatesButton_Click(object sender, RoutedEventArgs e)
        {
            OB.Logger.LogInfo(Components.ProxyManager, "Deleting duplicate proxies");

            vm.RemoveDuplicates();
            vm.UpdateProperties();
        }

        private void DeleteUntestedButton_Click(object sender, RoutedEventArgs e)
        {
            OB.Logger.LogInfo(Components.ProxyManager, "Deleting all untested proxies");

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
                            OB.Logger.LogError(Components.ProxyManager, "Failed to parse proxies type from file name, defaulting to HTTP");
                            AddProxies(lines);
                        }
                    }
                    catch (Exception ex)
                    {
                        OB.Logger.LogError(Components.ProxyManager, $"Failed to open file {file} - {ex.Message}");
                    }
                }
            }
        }

        private void copySelectedProxies_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Selected.CopyToClipboard(p => $"{p.Host}:{p.Port}");
                OB.Logger.LogInfo(Components.ProxyManager, $"Copied {Selected.Count()} proxies");
            }
            catch (Exception ex)
            {
                OB.Logger.LogError(Components.ProxyManager, $"Failed to copy proxies - {ex.Message}");
            }
        }

        private void copySelectedProxiesFull_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Selected.CopyToClipboard(p => $"({p.Type}){p.Host}:{p.Port}" + (string.IsNullOrEmpty(p.Username) ? "" : $"{p.Username}:{p.Password}"));
                OB.Logger.LogInfo(Components.ProxyManager, $"Copied {Selected.Count()} proxies");
            }
            catch (Exception ex)
            {
                OB.Logger.LogError(Components.ProxyManager, $"Failed to copy proxies - {ex.Message}");
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
