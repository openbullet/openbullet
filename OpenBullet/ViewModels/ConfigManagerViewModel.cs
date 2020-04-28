using Extreme.Net;
using OpenBullet.Models;
using OpenBullet.Repositories;
using PluginFramework;
using RuriLib;
using RuriLib.Functions.Formats;
using RuriLib.Interfaces;
using RuriLib.LS;
using RuriLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace OpenBullet.ViewModels
{
    public class ConfigManagerViewModel : ViewModelBase, IConfigManager
    {
        private ConfigRepository _diskRepo;

        private ObservableCollection<ConfigViewModel> configsCollection;
        public ObservableCollection<ConfigViewModel> ConfigsCollection 
        { 
            get => configsCollection;
            private set
            {
                configsCollection = value;
                OnPropertyChanged(); // We need to raise this when we create a new collection or it will not show up!
            }
        }

        public IEnumerable<ConfigViewModel> Configs => ConfigsCollection;

        public int Total => ConfigsCollection.Count;

        public int SavedHash { get; set; } = 0;

        private Config hoveredConfig;
        public Config HoveredConfig { get => hoveredConfig; set { hoveredConfig = value; OnPropertyChanged(); } }

        private ConfigViewModel currentConfig;
        public ConfigViewModel CurrentConfig 
        { 
            get => currentConfig;
            set 
            {
                currentConfig = value;
                OnPropertyChanged(nameof(CurrentConfigName));
            } 
        }

        public string CurrentConfigName => CurrentConfig == null ? "None" : CurrentConfig.Name;

        public ConfigManagerViewModel()
        {
            _diskRepo = new ConfigRepository(OB.configFolder);
            Rescan();
        }

        public IEnumerable<string> GetRequiredPlugins(ConfigViewModel config)
        {
            return new LoliScript(config.Config.Script)
                .ToBlocks()
                .OnlyPlugins()
                .Cast<IBlockPlugin>()
                .Select(p => p.Name)
                .Distinct();
        }

        #region Filters
        private string searchString = "";
        public string SearchString 
        {
            get => searchString;
            set 
            { 
                searchString = value;
                OnPropertyChanged();
                CollectionViewSource.GetDefaultView(ConfigsCollection).Refresh();
                OnPropertyChanged(nameof(Total)); 
            }
        }

        public void HookFilters()
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ConfigsCollection);
            view.Filter = ConfigsFilter;
        }

        private bool ConfigsFilter(object item)
        {
            return (item as ConfigViewModel).Name.ToLower().Contains(searchString.ToLower());
        }
        #endregion

        #region Get from disk
        public List<ConfigViewModel> GetConfigsFromDisk(bool sort = false, bool reverse = false)
        {
            var configs = _diskRepo.Get().ToList();

            if (sort)
            {
                configs.Sort((m1, m2) => m1.Config.Settings.LastModified.CompareTo(m2.Config.Settings.LastModified));

                if (reverse)
                {
                    configs.Reverse();
                }
            }
            return configs;
        }
        #endregion

        #region Get from sources
        public IEnumerable<ConfigViewModel> GetConfigsFromSources()
        {
            var configs = new List<ConfigViewModel>();

            foreach (var source in OB.OBSettings.Sources.Sources)
            {
                try
                {
                    configs.AddRange(PullSource(source));
                }
                catch (Exception ex)
                {
                    OB.Logger.LogError(Components.ConfigManager, $"Error with API {source.ApiUrl}\r\nReason: {ex.Message}", true);
                }
            }

            return configs;
        }

        public IEnumerable<ConfigViewModel> PullSource(Source source)
        {
            using (var wc = new WebClient())
            {
                var configs = new List<ConfigViewModel>();

                switch (source.Auth)
                {
                    case Source.AuthMode.ApiKey:
                        wc.Headers.Add(HttpRequestHeader.Authorization, source.ApiKey);
                        break;

                    case Source.AuthMode.UserPass:
                        var header = ($"{source.Username}:{source.Password}").ToBase64();
                        wc.Headers.Add(HttpRequestHeader.Authorization, $"Basic {header}");
                        break;

                    default:
                        break;
                }

                byte[] file = new byte[] { };
                file = wc.DownloadData(source.ApiUrl);

                var status = wc.ResponseHeaders["Result"];
                if (status != null && status == "Error")
                {
                    throw new Exception($"The server says: {Encoding.ASCII.GetString(file)}");
                }

                try
                {
                    using (var zip = new ZipArchive(new MemoryStream(file), ZipArchiveMode.Read))
                    {
                        foreach (var entry in zip.Entries)
                        {
                            var subCategory = Path.GetDirectoryName(entry.FullName).Replace("\\", " - ");
                            var category = subCategory == string.Empty ? "Remote" : $"Remote - {subCategory}";
                            using (var stream = entry.Open())
                            {
                                using (TextReader tr = new StreamReader(stream))
                                {
                                    var text = tr.ReadToEnd();
                                    var cfg = IOManager.DeserializeConfig(text);
                                    configs.Add(new ConfigViewModel("", category, cfg, true));
                                }
                            }
                        }
                    }
                }
                catch { }

                return configs;
            }
        }
        #endregion

        #region CRUD Operations
        // Create
        public void Add(ConfigViewModel config)
        {
            _diskRepo.Add(config);
            ConfigsCollection.Add(config);
        }

        // Read
        public void Rescan()
        {
            ConfigsCollection = new ObservableCollection<ConfigViewModel>(
                GetConfigsFromDisk(true, true)
                .Concat(GetConfigsFromSources()));

            HookFilters();

            OnPropertyChanged(nameof(Total));
        }

        // Update
        public void Update(ConfigViewModel config)
        {
            _diskRepo.Update(config);
        }

        public void SaveCurrent()
        {
            Update(CurrentConfig);
        }

        // Delete
        public void Remove(ConfigViewModel config)
        {
            _diskRepo.Remove(config);
            ConfigsCollection.Remove(config);
        }

        public void Remove(IEnumerable<ConfigViewModel> configs)
        {
            var toRemove = configs.ToArray();
            foreach (var config in toRemove)
            {
                ConfigsCollection.Remove(config);
            }

            _diskRepo.Remove(toRemove);
        }
        #endregion
    }
}
