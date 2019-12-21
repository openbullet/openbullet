using RuriLib;
using System.Windows.Controls;

namespace OpenBullet.Views.Main.Configs.OtherOptions
{
    /// <summary>
    /// Logica di interazione per Selenium.xaml
    /// </summary>
    public partial class Selenium : Page
    {
        public Selenium()
        {
            InitializeComponent();
            DataContext = OB.ConfigManager.CurrentConfig.Config.Settings;
        }
    }
}
