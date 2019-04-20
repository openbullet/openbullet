using RuriLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpenBullet.ViewModels
{
    public class OBSettingsSounds : ViewModelBase
    {
        private bool enableSounds = false;
        public bool EnableSounds { get { return enableSounds; } set { enableSounds = value; OnPropertyChanged(); } }
        private string onHitSound = "rifle_hit.wav";
        public string OnHitSound { get { return onHitSound; } set { onHitSound = value; OnPropertyChanged(); } }
        private string onReloadSound = "rifle_reload.wav";
        public string OnReloadSound { get { return onReloadSound; } set { onReloadSound = value; OnPropertyChanged(); } }

        public void Reset()
        {
            OBSettingsSounds def = new OBSettingsSounds();
            IList<PropertyInfo> props = new List<PropertyInfo>(typeof(OBSettingsSounds).GetProperties());

            foreach (PropertyInfo prop in props)
            {
                prop.SetValue(this, prop.GetValue(def, null));
            }
        }
    }
}
