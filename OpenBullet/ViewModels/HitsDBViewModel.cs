using LiteDB;
using OpenBullet.Repositories;
using RuriLib.Interfaces;
using RuriLib.Models;
using RuriLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;

namespace OpenBullet.ViewModels
{
    public class HitsDBViewModel : ViewModelBase, IHitsDB
    {
        public LiteDBRepository<Hit> _repo;
        public ObservableCollection<Hit> HitsCollection { get; private set; }

        public List<string> ConfigsList => HitsCollection.Select(x => x.ConfigName).Distinct().ToList();

        public int Total => HitsCollection.Count;

        private string searchString = "";
        public string SearchString { get { return searchString; } set { searchString = value; OnPropertyChanged("SearchString"); CollectionViewSource.GetDefaultView(HitsCollection).Refresh(); OnPropertyChanged("FilteredCount"); } }

        private string typeFilter = "SUCCESS";
        public string TypeFilter { get { return typeFilter; } set { typeFilter = value; OnPropertyChanged("TypeFilter"); CollectionViewSource.GetDefaultView(HitsCollection).Refresh(); OnPropertyChanged("FilteredCount"); } }

        private string configFilter = "All";
        public string ConfigFilter { get { return configFilter; } set { configFilter = value; OnPropertyChanged("ConfigFilter"); CollectionViewSource.GetDefaultView(HitsCollection).Refresh(); OnPropertyChanged("FilteredCount"); } }

        public int FilteredCount => HitsCollection.Where(h => HitsFilter(h)).Count();

        public IEnumerable<Hit> Hits => HitsCollection;

        public HitsDBViewModel()
        {
            _repo = new LiteDBRepository<Hit>("hits");
            HitsCollection = new ObservableCollection<Hit>();

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(HitsCollection);
            HookFilters();
        }

        public void HookFilters()
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(HitsCollection);
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
            HitsCollection = new ObservableCollection<Hit>(_repo.Get());

            HookFilters();

            OnPropertyChanged("Total");
            OnPropertyChanged("HitsCount");
        }

        public void RemoveAll()
        {
            _repo.RemoveAll();
            HitsCollection.Clear();
        }

        public void Remove(Hit hit)
        {
            _repo.Remove(hit);
            HitsCollection.Remove(hit);
        }

        public void DeleteDuplicates()
        {
            var groups = _repo.Get()
                    .GroupBy(h => h.GetHashCode())
                    .Where(g => g.Count() > 1)
                    .Select(g => g.OrderBy(h => h.Date).Reverse().Skip(1));

            foreach (var group in groups)
            {
                var list = group.ToList();
                Hit curr = null;
                while ((curr = list.FirstOrDefault()) != null)
                {
                    _repo.Remove(curr);

                    // Remove the actual reference to the hit (curr is from a cloned list, generated via Select LINQ method)
                    HitsCollection.Remove(HitsCollection.First(h => h.Id == curr.Id)); 
                    list.RemoveAt(0); // Remove from list of items to delete
                }
            }
        }

        public void DeleteFiltered()
        {
            var list = HitsCollection.Where(h =>
                    (string.IsNullOrEmpty(SearchString) ? true : h.CapturedString.ToLower().Contains(SearchString.ToLower())) &&
                    (ConfigFilter == "All" ? true : h.ConfigName == ConfigFilter) &&
                    h.Type == TypeFilter).ToList();

            foreach (var hit in list)
            {
                Remove(hit);
            }
        }

        public void Update(Hit hit)
        {
            _repo.Update(hit);
        }

        public void Add(Hit hit)
        {
            HitsCollection.Add(hit);
            _repo.Add(hit);
        }
    }
}
