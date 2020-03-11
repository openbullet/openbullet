using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginFramework.Attributes
{
    public class WordlistPicker : InputField
    {
        public WordlistPicker(string label, string tooltip = "") : base(label, tooltip) { }
    }
}
