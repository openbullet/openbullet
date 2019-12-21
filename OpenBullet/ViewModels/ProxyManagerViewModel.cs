using Extreme.Net;
using LiteDB;
using Newtonsoft.Json.Linq;
using OpenBullet.Repositories;
using RuriLib.Interfaces;
using RuriLib.Models;
using RuriLib.Models.Stats;
using RuriLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenBullet.ViewModels
{
    public class ProxyManagerViewModel : ViewModelBase, IProxyManager, IProxyChecker
    {
        private LiteDBRepository<CProxy> _repo;

        private ObservableCollection<CProxy> _proxiesCollection;
        public ObservableCollection<CProxy> ProxiesCollection 
        { 
            get
            { 
                return _proxiesCollection;
            } 
            private set 
            { 
                _proxiesCollection = value;
                UpdateProperties(); 
            }
        }

        public int Total => ProxiesCollection.Count;

        public IEnumerable<CProxy> Proxies => ProxiesCollection;

        public ProxyManagerViewModel()
        {
            _repo = new LiteDBRepository<CProxy>(Globals.dataBaseFile, "proxies");
            ProxiesCollection = new ObservableCollection<CProxy>();
        }

        #region Statistics
        public int Tested => ProxiesCollection.Count(x => x.Working != ProxyWorking.UNTESTED);
        public int Http => ProxiesCollection.Count(x => x.Type == ProxyType.Http);
        public int Socks4 => ProxiesCollection.Count(x => x.Type == ProxyType.Socks4);
        public int Socks4a => ProxiesCollection.Count(x => x.Type == ProxyType.Socks4a);
        public int Socks5 => ProxiesCollection.Count(x => x.Type == ProxyType.Socks5);
        public int Chain => ProxiesCollection.Count(x => x.Type == ProxyType.Chain);
        public int Working => ProxiesCollection.Count(x => x.Working == ProxyWorking.YES);
        public int NotWorking => ProxiesCollection.Count(x => x.Working == ProxyWorking.NO);

        public void UpdateProperties()
        {
            OnPropertyChanged(nameof(Total));
            OnPropertyChanged(nameof(Tested));
            OnPropertyChanged(nameof(Http));
            OnPropertyChanged(nameof(Socks4));
            OnPropertyChanged(nameof(Socks4));
            OnPropertyChanged(nameof(Socks5));
            OnPropertyChanged(nameof(Chain));
            OnPropertyChanged(nameof(Working));
            OnPropertyChanged(nameof(NotWorking));
        }

        public ProxyManagerStats Stats => new ProxyManagerStats(Total, Tested, Working, Http, Socks4, Socks4a, Socks5);
        #endregion

        #region Checker
        private int botsAmount = 1;
        public int BotsAmount { get { return botsAmount; } set { botsAmount = value; OnPropertyChanged(); } }

        private string testSite = "https://google.com";
        public string TestSite { get { return testSite; } set { testSite = value; OnPropertyChanged(); } }

        private string successKey = "title>Google";
        public string SuccessKey { get { return successKey; } set { successKey = value; OnPropertyChanged(); } }

        private bool onlyUntested = true;
        public bool OnlyUntested { get { return onlyUntested; } set { onlyUntested = value; OnPropertyChanged(); } }

        private int timeout = 2;
        public int Timeout { get { return timeout; } set { timeout = value; OnPropertyChanged(); } }

        public static readonly int maximumBots = 200;

        public async Task CheckAllAsync(IEnumerable<CProxy> proxies, CancellationToken cancellationToken, Action<CheckResult<ProxyResult>> onResult = null, IProgress<float> progress = null)
        {
            using (var ss = new SemaphoreSlim(BotsAmount, BotsAmount))
            {
                var total = proxies.Count();
                var current = 0;

                // Build the task list
                var tasks = proxies.Select(async proxy =>
                {
                    // Wait for the semaphore
                    await ss.WaitAsync();

                    CheckResult<ProxyResult> checkResult = new CheckResult<ProxyResult>();
                    ProxyResult proxyResult = new ProxyResult();
                    proxyResult.proxy = proxy;
                    
                    // Check the proxy
                    try
                    {
                        proxyResult = await CheckProxy(proxy);
                        checkResult = new CheckResult<ProxyResult>(true, proxyResult);
                    }
                    // Catch and log any errors
                    catch (Exception ex)
                    {
                        checkResult = new CheckResult<ProxyResult>(false, proxyResult, ex.Message);
                    }
                    // Report the progress and release the semaphore slot
                    finally
                    {
                        onResult?.Invoke(checkResult);
                        progress?.Report((float)++current / total);
                        ss.Release();
                    }
                });

                await Task.WhenAny(Task.WhenAll(tasks), AsTask(cancellationToken));
                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        public async Task<ProxyResult> CheckAsync(CProxy proxy, CancellationToken cancellationToken)
        {
            var task = CheckProxy(proxy);
            await Task.WhenAny(task, AsTask(cancellationToken));

            cancellationToken.ThrowIfCancellationRequested();
            return task.Result;
        }

        private async Task<ProxyResult> CheckProxy(CProxy proxy)
        {
            ProxyResult result = new ProxyResult();
            result.proxy = proxy;
            
            var sw = new Stopwatch();
            sw.Start();

            result.working = await CheckWorking(proxy);

            sw.Stop();
            
            result.ping = (int)sw.ElapsedMilliseconds;

            // Try to check the country (it's not essential)
            try
            {
                result.country = await CheckCountry(proxy);
            }
            catch { }

            return result;
        }

        private async Task<bool> CheckWorking(CProxy proxy)
        {
            var timeout = Timeout * 1000;
            using (var request = new HttpRequest())
            {
                request.Proxy = proxy.GetClient();
                request.Proxy.ConnectTimeout = timeout;
                request.Proxy.ReadWriteTimeout = timeout;
                request.ConnectTimeout = timeout;
                request.KeepAliveTimeout = timeout;
                request.ReadWriteTimeout = timeout;
                var response = await request.GetAsync(TestSite);
                var source = response.ToString();

                return source.Contains(SuccessKey);
            }
        }

        private async Task<string> CheckCountry(CProxy proxy)
        {
            using (var request = new HttpRequest())
            {
                request.ConnectTimeout = Timeout;
                var response = await request.GetAsync("http://ip-api.com/json/" + proxy.Host);
                var json = JObject.Parse(response.ToString());
                var status = json.Value<string>("status");

                if (status == "success")
                {
                    return json.Value<string>("country");
                }
                else
                {
                    return "Unknown";
                }
            }
        }

        public static Task AsTask(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<object>();
            cancellationToken.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);
            return tcs.Task;
        }
        #endregion

        #region CRUD Operations
        // Create
        public void Add(CProxy proxy)
        {
            ProxiesCollection.Add(proxy);
            _repo.Add(proxy);
        }

        public void AddRange(IEnumerable<CProxy> proxies)
        {
            foreach (var p in proxies)
            {
                ProxiesCollection.Add(p);
            }

            _repo.Add(proxies);
        }

        // Read
        public void RefreshList()
        {
            ProxiesCollection = new ObservableCollection<CProxy>(_repo.Get());
        }

        // Update
        public void Update(CProxy proxy)
        {
            _repo.Update(proxy);
        }

        // Delete
        public void Remove(CProxy proxy)
        {
            ProxiesCollection.Remove(proxy);
            _repo.Remove(proxy);
        }

        public void Remove(IEnumerable<CProxy> proxies)
        {
            foreach (var proxy in proxies.ToArray())
            {
                ProxiesCollection.Remove(proxy);
            }

            _repo.Remove(proxies);
        }

        public void RemoveAll()
        {
            ProxiesCollection.Clear();
            _repo.RemoveAll();
        }
        #endregion

        #region Delete methods
        public void RemoveNotWorking()
        {
            Remove(Proxies.Where(p => p.Working == ProxyWorking.NO));
        }

        public void RemoveDuplicates()
        {
            var duplicates = Proxies
                .GroupBy(p => p.Proxy)
                .Where(g => g.Count() > 1)
                .SelectMany(g => g.OrderBy(p => p.Proxy).Reverse().Skip(1));

            Remove(duplicates);
        }

        public void RemoveUntested()
        {
            Remove(Proxies.Where(p => p.Working == ProxyWorking.UNTESTED));
        }
        #endregion
    }
}
