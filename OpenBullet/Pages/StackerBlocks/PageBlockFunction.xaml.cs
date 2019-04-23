using RuriLib;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace OpenBullet.Pages.StackerBlocks
{
    /// <summary>
    /// Logica di interazione per PageBlockFunction.xaml
    /// </summary>
    public partial class PageBlockFunction : Page
    {
        BlockFunction vm;

        public PageBlockFunction(BlockFunction block)
        {
            InitializeComponent();
            vm = block;
            DataContext = vm;

            foreach (var t in Enum.GetNames(typeof(BlockFunction.Function)))
                functionTypeCombobox.Items.Add(t);

            functionTypeCombobox.SelectedIndex = (int)vm.FunctionType;

            foreach (var h in Enum.GetNames(typeof(BlockFunction.Hash)))
            {
                hashTypeCombobox.Items.Add(h);
                hmacHashTypeCombobox.Items.Add(h);
            }

            hashTypeCombobox.SelectedIndex = (int)vm.HashType;
            hmacHashTypeCombobox.SelectedIndex = (int)vm.HashType;

            dictionaryRTB.AppendText(vm.GetDictionary());
        }

        private void functionTypeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.FunctionType = (BlockFunction.Function)((ComboBox)e.OriginalSource).SelectedIndex;
            try { functionInfoTextblock.Text = infoDic[vm.FunctionType.ToString()]; } catch { functionInfoTextblock.Text = "No additional information available for this function"; }

            switch (vm.FunctionType)
            {
                default:
                    functionTabControl.SelectedIndex = 0;
                    break;

                case BlockFunction.Function.Hash:
                    functionTabControl.SelectedIndex = 1;
                    break;

                case BlockFunction.Function.HMAC:
                    functionTabControl.SelectedIndex = 2;
                    break;

                case BlockFunction.Function.Translate:
                    functionTabControl.SelectedIndex = 3;
                    break;

                case BlockFunction.Function.DateToUnixTime:
                    functionTabControl.SelectedIndex = 4;
                    break;

                case BlockFunction.Function.Replace:
                    functionTabControl.SelectedIndex = 5;
                    break;

                case BlockFunction.Function.RegexMatch:
                    functionTabControl.SelectedIndex = 6;
                    break;

                case BlockFunction.Function.RandomNum:
                    functionTabControl.SelectedIndex = 7;
                    break;

                case BlockFunction.Function.CountOccurrences:
                    functionTabControl.SelectedIndex = 8;
                    break;

                case BlockFunction.Function.RSA:
                    functionTabControl.SelectedIndex = 9;
                    break;

                case BlockFunction.Function.CharAt:
                    functionTabControl.SelectedIndex = 10;
                    break;

                case BlockFunction.Function.Substring:
                    functionTabControl.SelectedIndex = 11;
                    break;

                case BlockFunction.Function.AESEncrypt:
                    functionTabControl.SelectedIndex = 12;
                    break;

                case BlockFunction.Function.AESDecrypt:
                    functionTabControl.SelectedIndex = 12;
                    break;
            }
        }

        private void hashTypeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.HashType = (BlockFunction.Hash)((ComboBox)e.OriginalSource).SelectedIndex;
        }

        private void hmacHashTypeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.HashType = (BlockFunction.Hash)((ComboBox)e.OriginalSource).SelectedIndex;
        }

        public Dictionary<string, string> infoDic = new Dictionary<string, string>()
        {
            { "Constant", "This will just return anything written in the input string and store it in a variable, after possibly replacing all the input variables.\nUse this to chain constants and variables together." },
            { "Hash", "The input string will be hashed with the selected function. Remember you can chain variables if you need a salt." },
            { "HMAC", "" },
            { "RandomString", "?l = Lowercase, ?u = Uppercase, ?d = Digit, ?s = Symbol, ?h = Hex (Lowercase), ?a = Any"},
            { "Translate", "Format like headers (this: that), one per line." },
            { "Compute", "Calculates the value of a math expression, for example (6+3)*5 will return 45." },
            { "RSA", "Thanks to TheLittleTrain17 for this implementation" },
            { "Delay", "Write the amount of MILLISECONDS you want to wait in the input field" },
            { "CharAt", "Returns the character at the specified index of the string in the input field" },
            { "AESEncrypt", "256-bit key" },
            { "AESDecrypt", "256-bit key" }
        };

        private void dictionaryRTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            vm.SetDictionary(dictionaryRTB.Lines());
        }
    }
}
