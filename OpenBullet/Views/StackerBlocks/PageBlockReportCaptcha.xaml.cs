using CaptchaSharp.Enums;
using RuriLib;
using System;
using System.Windows.Controls;

namespace OpenBullet.Views.StackerBlocks
{
    /// <summary>
    /// Logica di interazione per BlockReportCaptchaPage.xaml
    /// </summary>
    public partial class PageBlockReportCaptcha : Page
    {
        BlockReportCaptcha vm;

        public PageBlockReportCaptcha(BlockReportCaptcha block)
        {
            InitializeComponent();
            vm = block;
            DataContext = vm;

            foreach (var t in Enum.GetNames(typeof(CaptchaType)))
                captchaTypeCombobox.Items.Add(t);

            captchaTypeCombobox.SelectedIndex = (int)vm.Type;
        }

        private void captchaTypeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.Type = (CaptchaType)((ComboBox)e.OriginalSource).SelectedIndex;
        }
    }
}
