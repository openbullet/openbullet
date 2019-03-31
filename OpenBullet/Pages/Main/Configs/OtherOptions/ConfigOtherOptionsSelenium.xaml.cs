using System.Windows.Controls;

namespace OpenBullet
{
    /// <summary>
    /// Logica di interazione per ConfigOtherOptionsSelenium.xaml
    /// </summary>
    public partial class ConfigOtherOptionsSelenium : Page
    {
        public ConfigOtherOptionsSelenium()
        {
            InitializeComponent();
            DataContext = Globals.mainWindow.ConfigsPage.CurrentConfig.Config.Settings;
        }
    }
}
