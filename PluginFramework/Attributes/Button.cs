using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginFramework.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class Button : Attribute
    {
        public string text;

        public Button(string text)
        {
            this.text = text;
        }
    }
}
