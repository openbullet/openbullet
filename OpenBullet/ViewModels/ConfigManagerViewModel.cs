using RuriLib;
using RuriLib.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;

namespace OpenBullet.ViewModels
{
    public class ConfigManagerViewModel : ViewModelBase
    {
        private ObservableCollection<ConfigViewModel> configsList;
        public ObservableCollection<ConfigViewModel> ConfigsList {
            get {
                return
                    SearchString == "" ?
                    configsList :
                    new ObservableCollection<ConfigViewModel>(configsList.Where(c => c.Name.ToLower().Contains(SearchString.ToLower())));
            }
            set { configsList = value; OnPropertyChanged("ConfigsList"); OnPropertyChanged("Total"); } }
        public int Total { get { return ConfigsList.Count; } }

        public string SavedConfig { get; set; }

        public Config moreInfoConfig;
        public Config MoreInfoConfig { get { return moreInfoConfig; } set { moreInfoConfig = value; OnPropertyChanged("MoreInfoConfig"); } }

        public string CurrentConfigName { get { return Globals.mainWindow.ConfigsPage.CurrentConfig.Name; } }

        private string searchString = "";
        public string SearchString { get { return searchString; } set { searchString = value; OnPropertyChanged("SearchString"); OnPropertyChanged("ConfigsList"); OnPropertyChanged("Total"); } }

        public ConfigManagerViewModel()
        {
            configsList = new ObservableCollection<ConfigViewModel>();            
            RefreshList();
        }

        public bool NameTaken(string name)
        {
            return ConfigsList.Any(x => x.Name == name);
        }

        public void RefreshCurrent()
        {
            OnPropertyChanged("CurrentConfigName");
        }

        public void RefreshList()
        {
            // Scan the directory for new configs
            ConfigsList = new ObservableCollection<ConfigViewModel>(GetConfigsFromDisk(true));

            OnPropertyChanged("Total");
        }

        public List<ConfigViewModel> GetConfigsFromDisk(bool sort = false)
        {
            List<ConfigViewModel> models = new List<ConfigViewModel>();

            // Load the configs in the root folder
            foreach(var file in Directory.EnumerateFiles(Globals.configFolder).Where(file => file.EndsWith(".loli")))
            {
                try { models.Add(new ConfigViewModel(file, "Default", IOManager.LoadConfig(file))); }
                catch { Globals.LogError(Components.ConfigManager, "Could not load file: " + file); }
            }

            // Load the configs in the subfolders
            foreach(var categoryFolder in Directory.EnumerateDirectories(Globals.configFolder))
            {
                foreach(var file in Directory.EnumerateFiles(categoryFolder).Where(file => file.EndsWith(".loli")))
                {
                    try { models.Add(new ConfigViewModel(file, System.IO.Path.GetFileName(categoryFolder), IOManager.LoadConfig(file))); }
                    catch { Globals.LogError(Components.ConfigManager, "Could not load file: " + file); }
                }
            }

            if (sort) { models.Sort((m1, m2) => m1.Config.Settings.LastModified.CompareTo(m2.Config.Settings.LastModified)); }
            return models;
        }
    }
}