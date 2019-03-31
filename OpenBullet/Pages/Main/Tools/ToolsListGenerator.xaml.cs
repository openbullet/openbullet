using Microsoft.Win32;
using OpenBullet.ViewModels;
using RuriLib.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet
{
    /// <summary>
    /// Logica di interazione per ToolsListGenerator.xaml
    /// </summary>
    public partial class ToolsListGenerator : Page
    {
        ListGeneratorViewModel vm = new ListGeneratorViewModel();
        StreamWriter sw;
        Random rand = new Random();

        public ToolsListGenerator()
        {
            InitializeComponent();
            DataContext = vm;
        }

        private void lowercaseButton_Click(object sender, RoutedEventArgs e)
        {
            vm.AllowedCharacters += "abcdefghijklmnopqrstuvwxyz";
        }

        private void uppercaseButton_Click(object sender, RoutedEventArgs e)
        {
            vm.AllowedCharacters += "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        }

        private void digitsButton_Click(object sender, RoutedEventArgs e)
        {
            vm.AllowedCharacters += "0123456789";
        }

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            vm.AllowedCharacters = "";
        }

        
        private void generateButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Text File |*.txt";
            sfd.Title = "Save Output List";
            sfd.ShowDialog();
            if(sfd.FileName != "")
            {
                sw = new StreamWriter(sfd.FileName);

                WriteCombinations(vm.Mask);

                sw.Close();
                sw.Dispose();

                if (vm.AutoImport)
                {
                    var wordlist = new Wordlist("Generated" + rand.Next(), sfd.FileName, "Default", "");
                    Globals.mainWindow.WordlistManagerPage.AddWordlist(wordlist);
                }
            }
        }

        /* Recursive method:
         * (AB**) <-- Starting list
         * (AB0*,AB1*)
         * (AB00,AB01,AB10,AB11) <-- End list
         * */

        private void WriteCombinations(string input)
        {
            if (input.Contains('*'))
            {
                foreach (char c in vm.AllowedCharacters)
                    WriteCombinations(new Regex("\\*").Replace(input, c.ToString(), 1));
            }
            else
            {
                // Get the card number without the :
                if ((vm.OnlyLuhn && Luhn(input.Split(':')[0])) || !vm.OnlyLuhn)
                    sw.WriteLine(input);
            }
        }

        
        private List<string> Generate(List<string> list)
        {
            if(list.Any(s => s.Contains('*'))){
                List<string> newList = new List<string>();
                foreach (string s in list)
                    foreach(char c in vm.AllowedCharacters)
                        newList.Add(new Regex("\\*").Replace(s, c.ToString(), 1));

                return Generate(newList);
            }
            else
                return list;
        }

        
        public static bool Luhn(string digits)
        {
            return digits.All(char.IsDigit) && digits.Reverse()
                .Select(c => c - 48)
                .Select((thisNum, i) => i % 2 == 0
                    ? thisNum
                    : ((thisNum *= 2) > 9 ? thisNum - 9 : thisNum)
                ).Sum() % 10 == 0;
        }
    }
}
