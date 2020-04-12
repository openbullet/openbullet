using OpenBullet.ViewModels;
using RuriLib;
using RuriLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet.Views.Main.Configs.OtherOptions
{
    /// <summary>
    /// Logica di interazione per Inputs.xaml
    /// </summary>
    public partial class Inputs : Page
    {
        ConfigSettings vm = null;
        Random rand = new Random();

        public Inputs()
        {
            vm = OB.ConfigManager.CurrentConfig.Config.Settings;
            DataContext = vm;

            InitializeComponent();
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
