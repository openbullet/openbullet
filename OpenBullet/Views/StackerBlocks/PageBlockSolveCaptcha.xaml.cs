using CaptchaSharp.Enums;
using RuriLib;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace OpenBullet.Views.StackerBlocks
{
    /// <summary>
    /// Logica di interazione per PageBlockSolveCaptcha.xaml
    /// </summary>
    public partial class PageBlockSolveCaptcha : Page
    {
        BlockSolveCaptcha vm;

        public PageBlockSolveCaptcha(BlockSolveCaptcha block)
        {
            InitializeComponent();
            vm = block;
            DataContext = vm;

            foreach (var t in Enum.GetNames(typeof(CaptchaType)))
                captchaTypeCombobox.Items.Add(t);

            captchaTypeCombobox.SelectedIndex = (int)vm.Type;

            foreach (var g in Enum.GetNames(typeof(CaptchaLanguageGroup)))
            {
                textLanguageGroupCombobox.Items.Add(g);
                imageLanguageGroupCombobox.Items.Add(g);
            }

            textLanguageGroupCombobox.SelectedIndex = (int)vm.LanguageGroup;
            imageLanguageGroupCombobox.SelectedIndex = (int)vm.LanguageGroup;

            foreach (var g in Enum.GetNames(typeof(CaptchaLanguage)))
            {
                textLanguageCombobox.Items.Add(g);
                imageLanguageCombobox.Items.Add(g);
            }

            textLanguageCombobox.SelectedIndex = (int)vm.Language;
            imageLanguageCombobox.SelectedIndex = (int)vm.Language;

            foreach (var s in Enum.GetNames(typeof(CharacterSet)))
                charSetCombobox.Items.Add(s);

            charSetCombobox.SelectedIndex = (int)vm.CharSet;
        }

        private void captchaTypeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.Type = (CaptchaType)((ComboBox)e.OriginalSource).SelectedIndex;

            var dict = new Dictionary<CaptchaType, int>
            {
                { CaptchaType.TextCaptcha,  0 },
                { CaptchaType.ImageCaptcha, 1 },
                { CaptchaType.ReCaptchaV2,  2 },
                { CaptchaType.ReCaptchaV3,  3 },
                { CaptchaType.FunCaptcha,   4 },
                { CaptchaType.HCaptcha,     5 },
                { CaptchaType.KeyCaptcha,   6 },
                { CaptchaType.GeeTest,      7 },
                { CaptchaType.Capy,         8 }
            };

            captchaTypeTabControl.SelectedIndex = dict[vm.Type];
        }

        private void languageGroupCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.LanguageGroup = (CaptchaLanguageGroup)((ComboBox)e.OriginalSource).SelectedIndex;
        }

        private void languageCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.Language = (CaptchaLanguage)((ComboBox)e.OriginalSource).SelectedIndex;
        }

        private void charSetCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.CharSet = (CharacterSet)((ComboBox)e.OriginalSource).SelectedIndex;
        }
    }
}
