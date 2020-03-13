using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginFramework.Attributes
{
    public class FilePicker : InputField
    {
        public string filter = "Text Files|*.txt";

        public FilePicker(string label, string tooltip = "") : base(label, tooltip) { }
    }
}
