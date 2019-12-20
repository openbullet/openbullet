using Extreme.Net;
using LiteDB;
using OpenBullet.Repositories;
using RuriLib.Interfaces;
using RuriLib.Models;
using RuriLib.Models.Stats;
using RuriLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading;

namespace OpenBullet.ViewModels
{
    public class ProxyManagerViewModel : ViewModelBase, IProxyManager
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
            OnPropertyChanged(nameof(Progress));
        }

        public ProxyManagerStats Stats => new ProxyManagerStats(Total, Tested, Working, Http, Socks4, Socks4a, Socks5);
        #endregion

        #region Checking
        public int Progress
        {
            get
            {
                var ret = 0;
                try { ret = (Tested * 100) / Total; } catch { } // If Size is 0 this will throw an Exception
                return ret;
            }
        }

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

        public bool IsBusy => throw new NotImplementedException();

        public void CheckAll(CancellationToken cancellationToken, IProgress<float> progress = null)
        {
            throw new NotImplementedException();
        }
        #endregion

        public IEnumerable<CProxy> Proxies => ProxiesCollection;

        public ProxyManagerViewModel()
        {
            _repo = new LiteDBRepository<CProxy>(Globals.dataBaseFile, "proxies");
            ProxiesCollection = new ObservableCollection<CProxy>();
        }

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
            foreach (var proxy in proxies)
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
            Remove(ProxiesCollection.Where(p => p.Working == ProxyWorking.NO));
        }

        public void RemoveDuplicates()
        {
            var duplicates = ProxiesCollection
                .GroupBy(p => p.Proxy)
                .Where(g => g.Count() > 1)
                .SelectMany(g => g.OrderBy(p => p.Proxy).Reverse().Skip(1));

            Remove(duplicates);
        }

        public void RemoveUntested()
        {
            Remove(ProxiesCollection.Where(p => p.Working == ProxyWorking.UNTESTED));
        }
        #endregion
    }
}
