using OpenBullet.ViewModels;
using RuriLib.ViewModels;
using System;
using System.Windows.Controls;

namespace OpenBullet.Views.Main.Settings.RL
{
    /// <summary>
    /// Logica di interazione per General.xaml
    /// </summary>
    public partial class General : Page
    {
        public General()
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
