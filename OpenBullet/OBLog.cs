using RuriLib;
using RuriLib.ViewModels;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace OpenBullet
{
    public class OBLog : ViewModelBase
    {
        public ObservableCollection<LogEntry> List { get; set; }
        private bool onlyErrors = false;
        public bool OnlyErrors { get { return onlyErrors; } set { onlyErrors = value; OnPropertyChanged("OnlyErrors"); Refresh(); } }
        private string searchString = "";
        public string SearchString { get { return searchString; } set { searchString = value; OnPropertyChanged("SearchString"); } }

        public OBLog()
        {
            List = new ObservableCollection<LogEntry>();

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(List);
            view.Filter = ErrorFilter;
        }

        public void Refresh()
        {
            try
            {
                CollectionViewSource.GetDefaultView(List).Refresh();
            }
            catch { }
        }

        private bool ErrorFilter(object item)
        {
            if (SearchString != "") // If search box not empty, filter out all the stuff that's not needed
            {
                if (!(item as LogEntry).LogString.ToLower().Contains(SearchString.ToLower()))
                    return false;
            }

            if (!Globals.log.OnlyErrors)
                return true;

            else
                return ((item as LogEntry).LogLevel == LogLevel.Error);
        }
    }
}
