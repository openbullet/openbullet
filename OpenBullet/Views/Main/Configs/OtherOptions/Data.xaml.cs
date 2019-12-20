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
        private ConfigSettings vm = Globals.mainWindow.ConfigsPage.CurrentConfig.Config.Settings;
        Random rand = new Random();

        public Data()
        {
            InitializeComponent();
            DataContext = vm;

            // Allowed Wordlist 1
            allowedWordlist1Combobox.Items.Add("");
            foreach (string i in Globals.environment.GetWordlistTypeNames())
                allowedWordlist1Combobox.Items.Add(i);

            try { allowedWordlist1Combobox.Text = vm.AllowedWordlist1; }
            catch { allowedWordlist1Combobox.SelectedIndex = 0; }

            // Allowed Wordlist 2
            allowedWordlist2Combobox.Items.Add("");
            foreach (string i in Globals.environment.GetWordlistTypeNames())
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

        private void ruleTypeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.GetDataRuleById((int)(sender as ComboBox).Tag).RuleType = (RuleType)(sender as ComboBox).SelectedIndex;
        }

        private void ruleStringCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox;
            vm.GetDataRuleById((int)cb.Tag).RuleString = cb.Text;
        }

        private void ruleTypeCombobox_Loaded(object sender, RoutedEventArgs e)
        {
            var dr = vm.GetDataRuleById((int)(sender as ComboBox).Tag);
            if (dr.TypeInitialized)
                return;

            dr.TypeInitialized = true;
            foreach (var t in Enum.GetNames(typeof(RuleType)))
                (sender as ComboBox).Items.Add(t);

            (sender as ComboBox).SelectedIndex = (int)dr.RuleType;
        }

        private void ruleStringCombobox_Loaded(object sender, RoutedEventArgs e)
        {
            var dr = vm.GetDataRuleById((int)(sender as ComboBox).Tag);
            if (dr.StringInitialized)
                return;

            dr.StringInitialized = true;

            var defaults = new string[] { "Lowercase", "Uppercase", "Digit", "Symbol" };

            foreach (var d in defaults)
                (sender as ComboBox).Items.Add(d);

            (sender as ComboBox).Text = dr.RuleString;
        }

        private void ruleStringCombobox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var cb = sender as ComboBox;
            vm.GetDataRuleById((int)cb.Tag).RuleString = cb.Text;
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
