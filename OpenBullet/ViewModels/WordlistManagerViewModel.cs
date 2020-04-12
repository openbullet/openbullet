using LiteDB;
using OpenBullet.Repositories;
using RuriLib.Interfaces;
using RuriLib.Models;
using RuriLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Data;

namespace OpenBullet.ViewModels
{
    public class WordlistManagerViewModel : ViewModelBase, IWordlistManager
    {
        private LiteDBRepository<Wordlist> _repo;

        private ObservableCollection<Wordlist> wordlistsCollection;
        public ObservableCollection<Wordlist> WordlistsCollection 
        { 
            get => wordlistsCollection;
            private set
            {
                wordlistsCollection = value;
                OnPropertyChanged();
            }
        }

        public int Total => WordlistsCollection.Count;

        public IEnumerable<Wordlist> Wordlists => WordlistsCollection;

        public WordlistManagerViewModel()
        {
            _repo = new LiteDBRepository<Wordlist>(OB.dataBaseFile, "wordlists");
            WordlistsCollection = new ObservableCollection<Wordlist>();
            RefreshList();
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
                CollectionViewSource.GetDefaultView(WordlistsCollection).Refresh();
                OnPropertyChanged(nameof(Total));
            }
        }

        public void HookFilters()
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(WordlistsCollection);
            view.Filter = WordlistsFilter;
        }

        private bool WordlistsFilter(object item)
        {
            return (item as Wordlist).Name.ToLower().Contains(searchString.ToLower());
        }
        #endregion

        public Wordlist GetWordlistByName(string name)
        {
            return WordlistsCollection.Where(x => x.Name == name).First();
        }

        public static Wordlist FileToWordlist(string path)
        {
            // Build the wordlist object
            var wordlist = new Wordlist(Path.GetFileNameWithoutExtension(path), path, OB.Settings.Environment.WordlistTypes.First().Name, "");

            // Get the first line
            var first = File.ReadLines(wordlist.Path).First();

            // Set the correct wordlist type
            wordlist.Type = OB.Settings.Environment.RecognizeWordlistType(first);

            return wordlist;
        }

        #region CRUD Operations
        // Create
        public void Add(Wordlist wordlist)
        {
            if (WordlistsCollection.Any(w => w.Path == wordlist.Path))
            {
                throw new Exception($"Wordlist already present: {wordlist.Path}");
            }

            WordlistsCollection.Add(wordlist);
            _repo.Add(wordlist);
        }

        // Read
        public void RefreshList()
        {
            WordlistsCollection = new ObservableCollection<Wordlist>(_repo.Get());
            HookFilters();
        }

        // Update
        public void Update(Wordlist wordlist)
        {
            _repo.Update(wordlist);
        }

        // Delete
        public void Remove(Wordlist wordlist)
        {
            WordlistsCollection.Remove(wordlist);
            _repo.Remove(wordlist);
        }

        public void RemoveAll()
        {
            WordlistsCollection.Clear();
            _repo.RemoveAll();
        }
        #endregion

        #region Delete methods
        public void DeleteNotFound()
        {
            for (var i = 0; i < WordlistsCollection.Count; i++)
            {
                var wordlist = WordlistsCollection[i];

                if (!File.Exists(wordlist.Path))
                {
                    Remove(wordlist);
                    i--;
                }
            }
        }
        #endregion
    }
}
