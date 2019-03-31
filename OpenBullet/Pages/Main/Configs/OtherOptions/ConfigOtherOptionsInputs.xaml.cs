using OpenBullet.ViewModels;
using RuriLib;
using RuriLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet
{
    /// <summary>
    /// Logica di interazione per ConfigOtherOptionsInput.xaml
    /// </summary>
    public partial class ConfigOtherOptionsInputs : Page
    {
        ConfigSettings vm = Globals.mainWindow.ConfigsPage.CurrentConfig.Config.Settings;
        Random rand = new Random();

        public ConfigOtherOptionsInputs()
        {
            InitializeComponent();
            DataContext = vm;
        }

        private void clearInputsButton_Click(object sender, RoutedEventArgs e)
        {
            vm.CustomInputs.Clear();
        }

        private void addInputButton_Click(object sender, RoutedEventArgs e)
        {
            vm.CustomInputs.Add(new CustomInput(rand.Next()));
        }

        private void removeInputButton_Click(object sender, RoutedEventArgs e)
        {
            vm.RemoveCustomInputById((int)(sender as Button).Tag);
        }
    }
}
