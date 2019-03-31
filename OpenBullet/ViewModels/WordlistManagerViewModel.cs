using LiteDB;
using RuriLib.Models;
using RuriLib.ViewModels;
using System.Collections.ObjectModel;

namespace OpenBullet.ViewModels
{
    class WordlistManagerViewModel : ViewModelBase
    {
        public ObservableCollection<Wordlist> WordlistList { get; set; }

        public int TotalWordlists { get { return WordlistList.Count; } }

        public WordlistManagerViewModel()
        {
            WordlistList = new ObservableCollection<Wordlist>();
        }
        
        public void RefreshList()
        {
            using (var db = new LiteDatabase(Globals.dataBaseFile))
            {
                WordlistList = new ObservableCollection<Wordlist>(db.GetCollection<Wordlist>("wordlists").FindAll());
            }
        }
    }
}
