using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginFramework.Attributes
{
    public class InfoText : InputField
    {
        public InfoText(string label, string tooltip = "") : base(label, tooltip) { }
    }
}
