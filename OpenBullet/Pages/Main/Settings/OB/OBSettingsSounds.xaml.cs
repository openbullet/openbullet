using RuriLib.ViewModels;
using System;
using System.Windows.Controls;

namespace OpenBullet
{
    /// <summary>
    /// Logica di interazione per OBSettingsSounds.xaml
    /// </summary>
    public partial class OBSettingsSounds : Page
    {
        public OBSettingsSounds()
        {
            InitializeComponent();
            DataContext = Globals.obSettings.Sounds;
        }
    }
}
