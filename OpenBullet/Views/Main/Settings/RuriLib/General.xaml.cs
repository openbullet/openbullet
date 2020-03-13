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
            DataContext = OB.Settings.RLSettings.General;

            foreach (string i in Enum.GetNames(typeof(BotsDisplayMode)))
                botsDisplayModeCombobox.Items.Add(i);

            botsDisplayModeCombobox.SelectedIndex = (int)OB.Settings.RLSettings.General.BotsDisplayMode;
        }

        private void botsDisplayModeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OB.Settings.RLSettings.General.BotsDisplayMode = (BotsDisplayMode)botsDisplayModeCombobox.SelectedIndex;
        }
    }
}
