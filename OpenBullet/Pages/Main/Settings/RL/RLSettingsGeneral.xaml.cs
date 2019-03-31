using OpenBullet.ViewModels;
using RuriLib.ViewModels;
using System;
using System.Windows.Controls;

namespace OpenBullet
{
    /// <summary>
    /// Logica di interazione per OBSettingsGeneral.xaml
    /// </summary>
    public partial class RLSettingsGeneral : Page
    {
        public RLSettingsGeneral()
        {
            InitializeComponent();
            DataContext = Globals.rlSettings.General;

            foreach (string i in Enum.GetNames(typeof(BotsDisplayMode)))
                botsDisplayModeCombobox.Items.Add(i);

            botsDisplayModeCombobox.SelectedIndex = (int)Globals.rlSettings.General.BotsDisplayMode;
        }

        private void botsDisplayModeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Globals.rlSettings.General.BotsDisplayMode = (BotsDisplayMode)botsDisplayModeCombobox.SelectedIndex;
        }
    }
}
