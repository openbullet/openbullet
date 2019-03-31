using RuriLib;
using RuriLib.Models;
using RuriLib.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace OpenBullet.ViewModels
{
    public class KeychainViewModel : ViewModelBase
    {
        private Random rand = new Random(3);

        private int id;
        public int Id { get { return id; } set { id = value; OnPropertyChanged(); } }

        public KeyChain Keychain { get; set; }
        public ObservableCollection<KeyViewModel> KeyList { get; set; } = new ObservableCollection<KeyViewModel>();

        public KeyChain.KeychainType Type { get { return Keychain.Type; } set { Keychain.Type = value; OnPropertyChanged(); OnPropertyChanged("KeychainColor"); OnPropertyChanged("CustomVisibility"); } }
        public bool TypeInitialized { get; set; }
        public KeyChain.KeychainMode Mode { get { return Keychain.Mode; } set { Keychain.Mode = value; OnPropertyChanged(); } }
        public bool ModeInitialized { get; set; }

        public string CustomType { get { return Keychain.CustomType; } set { Keychain.CustomType = value; OnPropertyChanged("CustomType"); OnPropertyChanged("KeychainColor"); } }
        public Visibility CustomVisibility { get { return Type == KeyChain.KeychainType.Custom ? Visibility.Visible : Visibility.Collapsed; } }
        public bool CustomTypeInitialized { get; set; }

        public SolidColorBrush KeychainColor { get
            {
                Color color = Colors.Black;
                switch (Type)
                {
                    case KeyChain.KeychainType.Success:
                        color = (Color)ColorConverter.ConvertFromString("#006600");
                        break;

                    case KeyChain.KeychainType.Failure:
                        color = (Color)ColorConverter.ConvertFromString("#cc0000");
                        break;

                    case KeyChain.KeychainType.Custom:
                        color = Globals.environment.GetCustomKeychain(CustomType).Color;
                        break;

                    case KeyChain.KeychainType.Ban:
                        color = (Color)ColorConverter.ConvertFromString("#660066");
                        break;

                    case KeyChain.KeychainType.Retry:
                        color = (Color)ColorConverter.ConvertFromString("#cc9900");
                        break;
                }
                return new SolidColorBrush(color);
            }
        }

        public KeyViewModel GetKeyById(int id)
        {
            return KeyList.Where(x => x.Id.KeyId == id).First();
        }

        public void RemoveKeyById(int id)
        {
            Keychain.Keys.Remove(GetKeyById(id).Key);
            KeyList.Remove(GetKeyById(id));
        }

        public void AddKey()
        {
            var key = new Key();
            Keychain.Keys.Add(key);
            KeyList.Add(new KeyViewModel(key, rand.Next(), Id));
        }

        public KeychainViewModel(KeyChain keychain, int id)
        {
            Keychain = keychain;
            Id = id;
            TypeInitialized = false;
            ModeInitialized = false;
            CustomTypeInitialized = false;
            foreach (Key key in keychain.Keys)
            {
                KeyList.Add(new KeyViewModel(key, rand.Next(), Id));
            }
        }
    }
}
