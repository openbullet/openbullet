using OpenBullet.Views.Main.Runner;
using RuriLib;
using RuriLib.Functions.Crypto;
using RuriLib.Functions.UserAgent;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet.Views.StackerBlocks
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

            foreach (var h in Enum.GetNames(typeof(Hash)))
            {
                hashTypeCombobox.Items.Add(h);
                hmacHashTypeCombobox.Items.Add(h);
                kdfAlgorithmCombobox.Items.Add(h);
            }

            hashTypeCombobox.SelectedIndex = (int)vm.HashType;
            hmacHashTypeCombobox.SelectedIndex = (int)vm.HashType;
            kdfAlgorithmCombobox.SelectedIndex = (int)vm.KdfAlgorithm;

            foreach (var b in Enum.GetNames(typeof(UserAgent.Browser)))
            {
                randomUABrowserCombobox.Items.Add(b);
            }

            randomUABrowserCombobox.SelectedIndex = (int)vm.UserAgentBrowser;

            foreach (var m in Enum.GetNames(typeof(CipherMode)))
            {
                aesModeCombobox.Items.Add(m);
            }

            aesModeCombobox.SelectedIndex = (int)vm.AesMode - 1;

            foreach (var p in Enum.GetNames(typeof(PaddingMode)))
            {
                aesPaddingCombobox.Items.Add(p);
            }

            aesPaddingCombobox.SelectedIndex = (int)vm.AesPadding - 1;

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

                case BlockFunction.Function.RSAEncrypt:
                    functionTabControl.SelectedIndex = 9;
                    break;

                    /*
                case BlockFunction.Function.RSADecrypt:
                    functionTabControl.SelectedIndex = 10;
                    break;
                    */

                case BlockFunction.Function.RSAPKCS1PAD2:
                    functionTabControl.SelectedIndex = 11;
                    break;

                case BlockFunction.Function.CharAt:
                    functionTabControl.SelectedIndex = 12;
                    break;

                case BlockFunction.Function.Substring:
                    functionTabControl.SelectedIndex = 13;
                    break;

                case BlockFunction.Function.GetRandomUA:
                    functionTabControl.SelectedIndex = 14;
                    break;

                case BlockFunction.Function.AESEncrypt:
                case BlockFunction.Function.AESDecrypt:
                    functionTabControl.SelectedIndex = 15;
                    break;

                case BlockFunction.Function.PBKDF2PKCS5:
                    functionTabControl.SelectedIndex = 16;
                    break;

                case BlockFunction.Function.UnixTimeToDate:
                    functionTabControl.SelectedIndex = 16;
                    break;
            }
        }

        public Dictionary<string, string> infoDic = new Dictionary<string, string>()
        {
            { "Constant", "This will just return anything written in the input string and store it in a variable, after possibly replacing all the input variables.\nUse this to chain constants and variables together." },
            { "Hash", "The input string will be hashed with the selected function. Remember you can chain variables if you need a salt." },
            { "HMAC", "" },
            { "RandomString", "?l = Lowercase, ?u = Uppercase, ?d = Digit, ?s = Symbol, ?h = Hex (Lowercase), ?m = Upper + Digits, ?i = Lower + Upper + Digits, ?a = Any"},
            { "Translate", "Format like headers (this: that), one per line." },
            { "Compute", "Calculates the value of a math expression, for example (6+3)*5 will return 45." },
            { "RSAEncrypt", "Encrypts data with RSA. All parameters must be provided as base64 strings" },
            { "RSADecrypt", "Decrypts data with RSA. All parameters must be provided as base64 strings" },
            { "RSAPKCS1PAD" , "Encrypts data with RSA Pkcs1Pad2. Modulus and exponent must be provided as HEX strings."},
            { "Delay", "Write the amount of MILLISECONDS you want to wait in the input field" },
            { "CharAt", "Returns the character at the specified index of the string in the input field" },
            { "AESEncrypt", "Encrypts data with AES. All parameters must be provided as base64 strings. Uses SHA-256 to get a 256 bit key" },
            { "AESDecrypt", "Decrypts data with AES. All parameters must be provided as base64 strings. Uses SHA-256 to get a 256 bit key" },
            { "PBKDF2PKCS5", "Generates a key based on a password. The salt, if provided, must be a base64 string" }
        };

        private void dictionaryRTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            vm.SetDictionary(dictionaryRTB.Lines());
        }

        private void hashTypeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.HashType = (Hash)((ComboBox)e.OriginalSource).SelectedIndex;
        }

        private void hmacHashTypeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.HashType = (Hash)((ComboBox)e.OriginalSource).SelectedIndex;
        }

        private void aesModeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.AesMode = (CipherMode)(((ComboBox)e.OriginalSource).SelectedIndex + 1);
        }

        private void aesPaddingCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.AesPadding = (PaddingMode)(((ComboBox)e.OriginalSource).SelectedIndex + 1);
        }

        private void kdfAlgorithmCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.KdfAlgorithm = (Hash)((ComboBox)e.OriginalSource).SelectedIndex;
        }

        private void randomUABrowserCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.UserAgentBrowser = (UserAgent.Browser)((ComboBox)e.OriginalSource).SelectedIndex;
        }
    }
}
