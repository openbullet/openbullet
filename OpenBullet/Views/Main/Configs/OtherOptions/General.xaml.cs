using RuriLib;
using System.Windows.Controls;

namespace OpenBullet.Views.Main.Configs.OtherOptions
{
    /// <summary>
    /// Logica di interazione per General.xaml
    /// </summary>
    public partial class General : Page
    {
        public General()
        {
            InitializeComponent();
            DataContext = OB.ConfigManager.CurrentConfig.Config.Settings;
        }
    }
}
