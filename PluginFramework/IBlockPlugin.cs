using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginFramework
{
    public interface IBlockPlugin
    {
        string Name { get; }

        string Color { get; }

        bool LightForeground { get; }
    }
}
