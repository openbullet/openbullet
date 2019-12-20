using RuriLib.ViewModels;
using System;
using System.Windows.Controls;

namespace OpenBullet.Views.Main.Settings.OB
{
    /// <summary>
    /// Logica di interazione per Sounds.xaml
    /// </summary>
    public partial class Sounds : Page
    {
        public Sounds()
        {
            InitializeComponent();
            DataContext = Globals.obSettings.Sounds;
        }
    }
}
