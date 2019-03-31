using RuriLib;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace OpenBullet.Pages.StackerBlocks
{
    /// <summary>
    /// Logica di interazione per PageBlockParse.xaml
    /// </summary>
    public partial class PageBlockParse : Page
    {
        BlockParse vm;

        public PageBlockParse(BlockParse block)
        {
            InitializeComponent();
            vm = block;
            DataContext = vm;
        }

        private void LRRadio_Checked(object sender, RoutedEventArgs e)
        {
            typeTabControl.SelectedIndex = 0;
        }

        private void CSSRadio_Checked(object sender, RoutedEventArgs e)
        {
            typeTabControl.SelectedIndex = 1;
        }

        private void JSONRadio_Checked(object sender, RoutedEventArgs e)
        {
            typeTabControl.SelectedIndex = 2;
        }

        private void REGEXRadio_Checked(object sender, RoutedEventArgs e)
        {
            typeTabControl.SelectedIndex = 3;
        }

        private void LRRTB_KeyUp(object sender, KeyEventArgs e)
        {
            // Sentry-like auto LR
            try
            {
                if (e.Key == Key.LeftShift)
                {
                    var begin = LRRTB.Document.ContentStart;
                    var start = (new TextRange(begin, LRRTB.Selection.Start)).Text.Length;
                    var len = LRRTB.Selection.Text.Length;
                    var end = start + len - 1;
                    var left = "";
                    var right = "";
                    var index = start;
                    do
                    {
                        if (index == 0) break;
                        left = LRRTB.GetText()[index - 1] + left;
                        index--;
                    }
                    while (BlockFunction.CountStringOccurrences(LRRTB.GetText(), left) > 1);
                    index = end;
                    do
                    {
                        if (index == LRRTB.GetText().Length - 1) break;
                        right += LRRTB.GetText()[index + 1];
                        index++;
                    }
                    while (BlockFunction.CountStringOccurrences(LRRTB.GetText(), right) > 1);
                    vm.LeftString = left;
                    vm.RightString = right;
                }
            }
            catch(Exception ex) { MessageBox.Show(ex.ToString()); }
        }


    }
}
