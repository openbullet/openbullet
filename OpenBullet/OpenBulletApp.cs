using OpenBullet.ViewModels;
using RuriLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenBullet
{
    public struct OpenBulletApp : IApplication
    {
        public IRunnerManager RunnerManager { get; set; }
        public IProxyManager ProxyManager { get; set; }
        public IProxyChecker ProxyChecker { get; set; }
        public IWordlistManager WordlistManager { get; set; }
        public IConfigManager ConfigManager { get; set; }
        public IHitsDB HitsDB { get; set; }
        public IAlerter Alerter { get; set; }
        public ILogger Logger { get; set; }
        public ISettings Settings { get; set; }
    }
}
