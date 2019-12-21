using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginFramework.Attributes
{
    public class Numeric : InputField
    {
        public int minimum = int.MinValue;
        public int maximum = int.MaxValue;

        public Numeric(string label, string tooltip = "") : base(label, tooltip) { }
    }
}
