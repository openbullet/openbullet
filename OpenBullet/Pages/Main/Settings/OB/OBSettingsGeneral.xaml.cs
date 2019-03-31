using OpenBullet.ViewModels;
using System;
using System.Windows.Controls;

namespace OpenBullet
{
    /// <summary>
    /// Logica di interazione per OBSettingsGeneral.xaml
    /// </summary>
    public partial class OBSettingsGeneral : Page
    {
        public OBSettingsGeneral()
        {
            InitializeComponent();
            DataContext = Globals.obSettings.General;
        }
    }
}
