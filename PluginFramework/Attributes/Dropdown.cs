using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginFramework.Attributes
{
    public class Dropdown : InputField
    {
        public string[] options = new string[] { };

        public Dropdown(string label, string tooltip = "") : base(label, tooltip) { }
    }
}
