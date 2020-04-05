using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginFramework.Attributes
{
    public class TextMulti : InputField
    {
        public bool readOnly = false;

        public TextMulti(string label, string tooltip = "") : base(label, tooltip) { }
    }
}
