using RuriLib;
using RuriLib.Models;
using System;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet.Views.Main.Configs.OtherOptions
{
    /// <summary>
    /// Logica di interazione per Data.xaml
    /// </summary>
    public partial class Data : Page
    {
        private ConfigSettings vm = null;
        Random rand = new Random();

        public Data()
        {
            vm = OB.ConfigManager.CurrentConfig.Config.Settings;
            DataContext = vm;
            
            InitializeComponent();

            // Allowed Wordlist 1
            allowedWordlist1Combobox.Items.Add("");
            foreach (string i in OB.Settings.Environment.GetWordlistTypeNames())
                allowedWordlist1Combobox.Items.Add(i);

            try { allowedWordlist1Combobox.Text = vm.AllowedWordlist1; }
            catch { allowedWordlist1Combobox.SelectedIndex = 0; }

            // Allowed Wordlist 2
            allowedWordlist2Combobox.Items.Add("");
            foreach (string i in OB.Settings.Environment.GetWordlistTypeNames())
                allowedWordlist2Combobox.Items.Add(i);

            try { allowedWordlist2Combobox.Text = vm.AllowedWordlist2; }
            catch { allowedWordlist2Combobox.SelectedIndex = 0; }
        }

        private void clearRulesButton_Click(object sender, RoutedEventArgs e)
        {
            vm.DataRules.Clear();
        }

        private void addRuleButton_Click(object sender, RoutedEventArgs e)
        {
            vm.DataRules.Add(new DataRule(rand.Next()));
        }

        private void removeRuleButton_Click(object sender, RoutedEventArgs e)
        {
            vm.RemoveDataRuleById((int)(sender as Button).Tag);
        }

        private void allowedWordlist1Combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.AllowedWordlist1 = (string)allowedWordlist1Combobox.SelectedValue;
        }

        private void allowedWordlist2Combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.AllowedWordlist2 = (string)allowedWordlist2Combobox.SelectedValue;
        }
    }
}
