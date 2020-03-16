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
        }

        public Wordlist GetWordlistByName(string name)
        {
            return WordlistsCollection.Where(x => x.Name == name).First();
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
                }
            }
        }
        #endregion
    }
}
