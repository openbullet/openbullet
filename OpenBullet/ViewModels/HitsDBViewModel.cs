﻿using LiteDB;
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

        private ObservableCollection<Hit> hitsCollection;
        public ObservableCollection<Hit> HitsCollection
        {
            get => hitsCollection;
            private set
            {
                hitsCollection = value;
                OnPropertyChanged();
            }
        }

        public int Total => HitsCollection.Count;

        public IEnumerable<Hit> Hits => HitsCollection;

        public HitsDBViewModel()
        {
            _repo = new LiteDBRepository<Hit>(OB.dataBaseFile, "hits");
            HitsCollection = new ObservableCollection<Hit>();

            HookFilters();
        }

        #region Filters
        public static readonly string defaultFilter = "All";

        public List<string> ConfigsList => HitsCollection.Select(x => x.ConfigName).Distinct().ToList();

        private string searchString = "";
        public string SearchString
        {
            get
            {
                return searchString;
            }
            set
            {
                searchString = value;
                OnPropertyChanged();
                CollectionViewSource.GetDefaultView(HitsCollection).Refresh();
                OnPropertyChanged(nameof(Filtered));
            }
        }

        private string typeFilter = "SUCCESS";
        public string TypeFilter
        {
            get
            {
                return typeFilter;
            }
            set
            {
                typeFilter = value;
                OnPropertyChanged();
                CollectionViewSource.GetDefaultView(HitsCollection).Refresh();
                OnPropertyChanged(nameof(Filtered));
            }
        }

        private string configFilter = defaultFilter;
        public string ConfigFilter
        {
            get
            {
                return configFilter;
            }
            set
            {
                configFilter = value;
                OnPropertyChanged();
                CollectionViewSource.GetDefaultView(HitsCollection).Refresh();
                OnPropertyChanged(nameof(Filtered));
            }
        }

        public int Filtered => HitsCollection.Count(h => HitsFilter(h));

        public void HookFilters()
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(HitsCollection);
            view.Filter = HitsFilter;
        }

        private bool HitsFilter(object item)
        {
            if ((item as Hit).Type != TypeFilter)
                return false;

            if (ConfigFilter != defaultFilter && (item as Hit).ConfigName != ConfigFilter)
                return false;
            
            if(!string.IsNullOrEmpty(SearchString))
                return ((item as Hit).CapturedData.ToCaptureString().ToLower().Contains(SearchString.ToLower())); // This is not very efficient!

            return true;
        }
        #endregion

        #region CRUD Operations
        // Create
        public void Add(Hit hit)
        {
            HitsCollection.Add(hit);
            _repo.Add(hit);
        }

        // Read
        public void RefreshList()
        {
            HitsCollection = new ObservableCollection<Hit>(_repo.Get());

            HookFilters();

            OnPropertyChanged(nameof(Total));
        }

        // Update
        public void Update(Hit hit)
        {
            _repo.Update(hit);
        }

        // Delete
        public void Remove(Hit hit)
        {
            HitsCollection.Remove(hit);
            _repo.Remove(hit);
        }

        public void Remove(IEnumerable<Hit> hits)
        {
            var toRemove = hits.ToArray();
            foreach (var hit in toRemove)
            {
                HitsCollection.Remove(hit);
            }

            _repo.Remove(toRemove);
        }

        public void RemoveAll()
        {
            HitsCollection.Clear();
            _repo.RemoveAll();
        }
        #endregion

        #region Delete methods
        public void DeleteDuplicates()
        {
            var duplicates = HitsCollection
                    .GroupBy(h => h.GetHashCode(OB.OBSettings.General.IgnoreWordlistOnHitDedupe))
                    .Where(g => g.Count() > 1)
                    .SelectMany(g => g.OrderBy(h => h.Date)
                    .Reverse().Skip(1)).ToList();

            Remove(duplicates);
        }

        public void DeleteFiltered()
        {
            var filtered = HitsCollection.Where(h =>
                    (string.IsNullOrEmpty(SearchString) ? true : h.CapturedString.ToLower().Contains(SearchString.ToLower())) &&
                    (ConfigFilter == "All" ? true : h.ConfigName == ConfigFilter) &&
                    h.Type == TypeFilter).ToList();

            Remove(filtered);
        }
        #endregion
    }
}
