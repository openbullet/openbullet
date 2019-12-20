using OpenBullet.ViewModels;
using System;
using System.Windows.Controls;

namespace OpenBullet.Views.Main.Settings.OB
{
    /// <summary>
    /// Logica di interazione per General.xaml
    /// </summary>
    public partial class General : Page
    {
        public General()
        {
            InitializeComponent();
            DataContext = Globals.obSettings.General;
        }
    }
}
