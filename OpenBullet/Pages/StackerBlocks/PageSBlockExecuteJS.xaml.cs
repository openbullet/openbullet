using ICSharpCode.AvalonEdit.Highlighting;
using RuriLib;
using System;
using System.Windows.Controls;

namespace OpenBullet.Pages.StackerBlocks
{
    /// <summary>
    /// Logica di interazione per PageSBlockExecuteJS.xaml
    /// </summary>
    public partial class PageSBlockExecuteJS : Page
    {
        SBlockExecuteJS vm;

        public PageSBlockExecuteJS(SBlockExecuteJS block)
        {
            InitializeComponent();
            vm = block;
            DataContext = vm;

            javascriptCodeEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("JavaScript");
            javascriptCodeEditor.ShowLineNumbers = true;
            javascriptCodeEditor.Text = vm.JavascriptCode;
        }

        private void javascriptCodeEditor_LostFocus(object sender, EventArgs e)
        {
            vm.JavascriptCode = javascriptCodeEditor.Text;
        }
    }
}
