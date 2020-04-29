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
            get => _proxiesCollection;
            private set 
            { 
                _proxiesCollection = value;
                OnPropertyChanged();
                UpdateProperties(); 
            }
        }

        public int Total => ProxiesCollection.Count;

        public IEnumerable<CProxy> Proxies => ProxiesCollection;

        public ProxyManagerViewModel()
        {
            _repo = new LiteDBRepository<CProxy>(OB.dataBaseFile, "proxies");
            ProxiesCollection = new ObservableCollection<CProxy>();
        }

        #region Statistics

        public object testedLock = new object();
        private int tested;
        public int Tested
        {
            set
            {
                tested = value;
                OnPropertyChanged();
            }
            get { return tested; }
        }

        public object httpLock = new object();
        private int http;
        public int Http
        {
            set
            {
                http = value;
                OnPropertyChanged();
            }
            get { return http; }
        }

        public object socks4Lock = new object();
        private int socks4;
        public int Socks4
        {
            set
            {
                socks4 = value;
                OnPropertyChanged();
            }
            get { return socks4; }
        }

        public object socks4aLock = new object();
        private int socks4a;
        public int Socks4a
        {
            set
            {
                socks4a = value;
                OnPropertyChanged();
            }
            get { return socks4a; }
        }

        public object socks5Lock = new object();
        private int socks5;
        public int Socks5
        {
            set
            {
                socks5 = value;
                OnPropertyChanged();
            }
            get { return socks5; }
        }

        public object chainLock = new object();
        private int chain;
        public int Chain
        {
            set
            {
                chain = value;
                OnPropertyChanged();
            }
            get { return chain; }
        }

        public object workingLock = new object();
        private int working;
        public int Working
        {
            set
            {
                working = value;
                OnPropertyChanged();
            }
            get { return working; }
        }

        public object notWorkingLock = new object();
        private int notWorking;
        public int NotWorking
        {
            set
            {
                notWorking = value;
                OnPropertyChanged();
            }
            get { return notWorking; }
        }

        public void UpdateProperties()
        {
            lock (testedLock)
                Tested = ProxiesCollection.Count(x => x.Working != ProxyWorking.UNTESTED);

            lock (httpLock)
                Http = ProxiesCollection.Count(x => x.Type == ProxyType.Http);

            lock (socks4Lock)
                Socks4 = ProxiesCollection.Count(x => x.Type == ProxyType.Socks4);

            lock (socks4aLock)
                Socks4a = ProxiesCollection.Count(x => x.Type == ProxyType.Socks4a);

            lock (socks5Lock)
                Socks5 = ProxiesCollection.Count(x => x.Type == ProxyType.Socks5);

            lock (chainLock)
                Chain = ProxiesCollection.Count(x => x.Type == ProxyType.Chain);

            lock (workingLock)
                Working = ProxiesCollection.Count(x => x.Working == ProxyWorking.YES);

            lock (notWorkingLock)
                NotWorking = ProxiesCollection.Count(x => x.Working == ProxyWorking.NO);
        }

        public ProxyManagerStats Stats => new ProxyManagerStats(Total, Tested, Working, Http, Socks4, Socks4a, Socks5);
        #endregion

        #region Checker
        private int botsAmount = 1;
        public int BotsAmount { get { return botsAmount; } set { botsAmount = value; OnPropertyChanged(); } }

        public string TestSite { get { return OB.Settings.ProxyManagerSettings.ActiveProxySiteUrl; } set { OB.Settings.ProxyManagerSettings.ActiveProxySiteUrl = value; OnPropertyChanged(); } }

        public string SuccessKey { get { return OB.Settings.ProxyManagerSettings.ActiveProxyKey; } set { OB.Settings.ProxyManagerSettings.ActiveProxyKey = value; OnPropertyChanged(); } }

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
            var toRemove = proxies.ToArray();
            foreach (var proxy in toRemove)
            {
                ProxiesCollection.Remove(proxy);
            }

            _repo.Remove(toRemove);
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
