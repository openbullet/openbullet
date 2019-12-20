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
    class WordlistManagerViewModel : ViewModelBase, IWordlistManager
    {
        private LiteDBRepository<Wordlist> _repo;
        public ObservableCollection<Wordlist> WordlistsCollection { get; private set; }

        public int Total => WordlistsCollection.Count;

        public IEnumerable<Wordlist> Wordlists => WordlistsCollection;

        public WordlistManagerViewModel()
        {
            WordlistsCollection = new ObservableCollection<Wordlist>();
            _repo = new LiteDBRepository<Wordlist>("wordlists");
        }

        public Wordlist GetWordlistByName(string name)
        {
            return WordlistsCollection.Where(x => x.Name == name).First();
        }

        public void Add(Wordlist wordlist)
        {
            if (WordlistsCollection.Any(w => w.Path == wordlist.Path))
            {
                throw new Exception($"Wordlist already present: {wordlist.Path}");
            }

            WordlistsCollection.Add(wordlist);
            _repo.Add(wordlist);
        }

        public void Delete(Wordlist wordlist)
        {
            WordlistsCollection.Remove(wordlist);
            _repo.Remove(wordlist);
        }

        public void DeleteNotFound()
        {
            foreach (var wordlist in WordlistsCollection)
            {
                if (!File.Exists(wordlist.Path))
                {
                    Remove(wordlist);
                }
            }
        }

        public void DeleteAll()
        {
            WordlistsCollection.Clear();
            _repo.RemoveAll();
        }

        public void RefreshList()
        {
            WordlistsCollection = new ObservableCollection<Wordlist>(_repo.Get());
        }

        public void Update(Wordlist wordlist)
        {
            _repo.Update(wordlist);
        }

        public void Remove(Wordlist wordlist)
        {
            _repo.Remove(wordlist);
        }
    }
}
