using RuriLib.ViewModels;
using System.Collections.ObjectModel;
using System.Linq;

namespace OpenBullet.ViewModels
{
    public class ConfigOtherOptionsInputViewModel : ViewModelBase
    {
        private ObservableCollection<CustomInput> inputsList = new ObservableCollection<CustomInput>();
        public ObservableCollection<CustomInput> InputsList { get { return inputsList; } set { inputsList = value; OnPropertyChanged("InputsList"); } }

        public CustomInput GetInputById(int id)
        {
            return InputsList.Where(x => x.Id == id).First();
        }

        public void RemoveInputById(int id)
        {
            InputsList.Remove(GetInputById(id));
        }
    }
}
