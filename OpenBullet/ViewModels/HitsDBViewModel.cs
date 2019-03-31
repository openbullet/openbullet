using LiteDB;
using RuriLib.Models;
using RuriLib.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;

namespace OpenBullet.ViewModels
{
    public class HitsDBViewModel : ViewModelBase
    {
        public ObservableCollection<Hit> HitsList { get; set; }

        public List<string> ConfigsList { get
            {
                return HitsList.Select(x => x.ConfigName).Distinct().ToList();
            }
        }

        public int Total { get { return HitsList.Count; } }

        private string searchString = "";
        public string SearchString { get { return searchString; } set { searchString = value; OnPropertyChanged("SearchString"); CollectionViewSource.GetDefaultView(HitsList).Refresh(); OnPropertyChanged("FilteredCount"); } }

        private string typeFilter = "SUCCESS";
        public string TypeFilter { get { return typeFilter; } set { typeFilter = value; OnPropertyChanged("TypeFilter"); CollectionViewSource.GetDefaultView(HitsList).Refresh(); OnPropertyChanged("FilteredCount"); } }

        private string configFilter = "All";
        public string ConfigFilter { get { return configFilter; } set { configFilter = value; OnPropertyChanged("ConfigFilter"); CollectionViewSource.GetDefaultView(HitsList).Refresh(); OnPropertyChanged("FilteredCount"); } }

        public HitsDBViewModel()
        {
            HitsList = new ObservableCollection<Hit>();

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(HitsList);
            HookFilters();
        }

        public int FilteredCount { get { return HitsList.Where(h => HitsFilter(h)).Count(); } }

        public void HookFilters()
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(HitsList);
            view.Filter = HitsFilter;
        }

        private bool HitsFilter(object item)
        {
            if ((item as Hit).Type != TypeFilter)
                return false;

            if (ConfigFilter != "All" && (item as Hit).ConfigName != ConfigFilter)
                return false;
            
            if(!string.IsNullOrEmpty(SearchString))
                return ((item as Hit).CapturedData.ToCaptureString().ToLower().Contains(SearchString.ToLower())); // This is not very efficient!

            return true;
        }

        public void RefreshList()
        {
            using (var db = new LiteDatabase(Globals.dataBaseFile))
            {
                HitsList = new ObservableCollection<Hit>(db.GetCollection<Hit>("hits").FindAll());
            }

            HookFilters();

            OnPropertyChanged("Total");
            OnPropertyChanged("HitsCount");
        }
    }
}
