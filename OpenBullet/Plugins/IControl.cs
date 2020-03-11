using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenBullet.Plugins
{
    public interface IControl
    {
        string Name { get; set; }
        dynamic GetValue();
        void SetValue(dynamic value);
    }
}
