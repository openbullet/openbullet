using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginFramework.Attributes
{
    public class Text : InputField
    {
        public bool readOnly = false;

        public Text(string label, string tooltip = "") : base(label, tooltip) { }
    }
}
