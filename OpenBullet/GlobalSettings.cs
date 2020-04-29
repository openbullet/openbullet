using RuriLib.Interfaces;
using RuriLib.Models;
using RuriLib.ViewModels;

namespace OpenBullet
{
    public class GlobalSettings : ISettings
    {
        public EnvironmentSettings Environment { get; set; }
        public RLSettingsViewModel RLSettings { get; set; }
        public ProxyManagerSettings ProxyManagerSettings { get; set; }
    }
}
