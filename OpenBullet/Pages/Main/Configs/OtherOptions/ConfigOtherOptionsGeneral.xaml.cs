using System.Windows.Controls;

namespace OpenBullet
{
    /// <summary>
    /// Logica di interazione per ConfigOtherOptionsGeneral.xaml
    /// </summary>
    public partial class ConfigOtherOptionsGeneral : Page
    {
        public ConfigOtherOptionsGeneral()
        {
            InitializeComponent();
            DataContext = Globals.mainWindow.ConfigsPage.CurrentConfig.Config.Settings;
        }
    }
}
