using OpenBullet.ViewModels;
using RuriLib;
using RuriLib.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpenBullet.Pages.StackerBlocks
{
    /// <summary>
    /// Logica di interazione per PageBlockKeycheck.xaml
    /// </summary>
    public partial class PageBlockKeycheck : Page
    {
        public BlockKeycheckViewModel vm;
        Random rand = new Random(1);

        public PageBlockKeycheck(BlockKeycheck block)
        {
            InitializeComponent();
            vm = new BlockKeycheckViewModel(block);
            DataContext = vm;
        }

        private void addKeychainImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            vm.AddKeychain();
        }

        private void keychainTypeCombobox_Loaded(object sender, RoutedEventArgs e)
        {
            var kc = vm.GetKeychainById((int)((ComboBox)e.OriginalSource).Tag);
            if (kc.TypeInitialized)
                return;

            kc.TypeInitialized = true;
            foreach (var t in Enum.GetNames(typeof(KeyChain.KeychainType)))
                ((ComboBox)e.OriginalSource).Items.Add(t);

            ((ComboBox)e.OriginalSource).SelectedIndex = (int)kc.Type;
        }

        private void keychainModeCombobox_Loaded(object sender, RoutedEventArgs e)
        {
            var kc = vm.GetKeychainById((int)((ComboBox)e.OriginalSource).Tag);
            if (kc.ModeInitialized)
                return;

            kc.ModeInitialized = true;
            foreach (var m in Enum.GetNames(typeof(KeyChain.KeychainMode)))
                ((ComboBox)e.OriginalSource).Items.Add(m);

            ((ComboBox)e.OriginalSource).SelectedIndex = (int)kc.Mode;
        }

        private void customTypeCombobox_Loaded(object sender, RoutedEventArgs e)
        {
            var kc = vm.GetKeychainById((int)((ComboBox)e.OriginalSource).Tag);
            if (kc.CustomTypeInitialized)
                return;

            kc.CustomTypeInitialized = true;
            foreach (var k in Globals.environment.GetCustomKeychainNames())
                ((ComboBox)e.OriginalSource).Items.Add(k);

            ((ComboBox)e.OriginalSource).SelectedValue = kc.CustomType;
        }

        private void keychainTypeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.GetKeychainById((int)((ComboBox)e.OriginalSource).Tag).Type = (KeyChain.KeychainType)((ComboBox)e.OriginalSource).SelectedIndex;
        }

        private void keychainModeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.GetKeychainById((int)((ComboBox)e.OriginalSource).Tag).Mode = (KeyChain.KeychainMode)((ComboBox)e.OriginalSource).SelectedIndex;
        }

        private void customTypeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.GetKeychainById((int)((ComboBox)e.OriginalSource).Tag).CustomType = (string)(sender as ComboBox).SelectedItem;
        }

        private void removeKeychainImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            vm.RemoveKeychainById((int)((Image)e.OriginalSource).Tag);
        }

        private void conditionCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tag = (KeyFullId)(((ComboBox)e.OriginalSource).Tag);
            vm.GetKeychainById(tag.ParentId).GetKeyById(tag.KeyId).Condition = (RuriLib.Condition)((ComboBox)e.OriginalSource).SelectedIndex;
        }

        private void conditionCombobox_Loaded(object sender, RoutedEventArgs e)
        {
            var tag = (KeyFullId)(((ComboBox)e.OriginalSource).Tag);
            if (tag.ConditionInitialized)
                return;

            tag.ConditionInitialized = true;
            foreach (var c in Enum.GetNames(typeof(RuriLib.Condition)))
                ((ComboBox)e.OriginalSource).Items.Add(c);

            ((ComboBox)e.OriginalSource).SelectedIndex = (int)vm.GetKeychainById(tag.ParentId).GetKeyById(tag.KeyId).Condition;
        }

        private void leftTermCombobox_Loaded(object sender, RoutedEventArgs e)
        {
            var tag = (KeyFullId)(((ComboBox)e.OriginalSource).Tag);
            if (tag.LeftTermInitialized)
                return;

            tag.LeftTermInitialized = true;
            var defaultFields = new string[] { "<SOURCE>", "<HEADERS(*)>", "<HEADERS{*}>", "<COOKIES(*)>", "<COOKIES{*}>", "<RESPONSECODE>", "<ADDRESS>" };
            foreach (var f in defaultFields)
                ((ComboBox)e.OriginalSource).Items.Add(f);
            
            try { ((ComboBox)e.OriginalSource).SelectedValue = vm.GetKeychainById(tag.ParentId).GetKeyById(tag.KeyId).LeftTerm; }
            catch { ((ComboBox)e.OriginalSource).SelectedIndex = 0; }
        }

        private void addKeyImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            vm.GetKeychainById((int)((Image)e.OriginalSource).Tag).AddKey();
        }

        private void removeKeyImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var tag = (KeyFullId)(((Image)e.OriginalSource).Tag);
            vm.GetKeychainById(tag.ParentId).RemoveKeyById(tag.KeyId);
        }
    }

    public class KeyFullId
    {
        public int KeyId { get; set; }
        public int ParentId { get; set; }
        public bool LeftTermInitialized { get; set; }
        public bool ConditionInitialized { get; set; }

        public KeyFullId()
        {
            KeyId = 0;
            ParentId = 0;
            LeftTermInitialized = false;
            ConditionInitialized = false;
        }
    }

}
