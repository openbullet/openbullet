using RuriLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenBullet.ViewModels
{
    public class OBSettingsSounds : ViewModelBase
    {
        private bool enableSounds = false;
        public bool EnableSounds { get { return enableSounds; } set { enableSounds = value; OnPropertyChanged(); } }
        private string onHitSound = "riflehit.wav";
        public string OnHitSound { get { return onHitSound; } set { onHitSound = value; OnPropertyChanged(); } }
        private string onReloadSound = "riflereload.wav";
        public string OnReloadSound { get { return onReloadSound; } set { onReloadSound = value; OnPropertyChanged(); } }
    }
}
