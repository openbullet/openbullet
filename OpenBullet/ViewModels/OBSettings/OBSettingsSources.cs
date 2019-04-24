using OpenBullet.Models;
using RuriLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpenBullet.ViewModels
{
    public class OBSettingsSources : ViewModelBase
    {
        public ObservableCollection<Source> Sources { get; set; } = new ObservableCollection<Source>();

        public void Reset()
        {
            OBSettingsSources def = new OBSettingsSources();
            IList<PropertyInfo> props = new List<PropertyInfo>(typeof(OBSettingsSources).GetProperties());

            foreach (PropertyInfo prop in props)
            {
                prop.SetValue(this, prop.GetValue(def, null));
            }
        }
    }
}
