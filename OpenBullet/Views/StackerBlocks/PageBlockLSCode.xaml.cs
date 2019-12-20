using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using RuriLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace OpenBullet.Pages.StackerBlocks
{
    /// <summary>
    /// Logica di interazione per PageBlockLSCode.xaml
    /// </summary>
    public partial class PageBlockLSCode : Page
    {
        BlockLSCode block;

        public PageBlockLSCode(BlockLSCode block)
        {
            InitializeComponent();
            this.block = block;
            DataContext = this.block;

            // Style the LoliScript editor
            scriptEditor.Text = block.Script;
            scriptEditor.ShowLineNumbers = true;
            scriptEditor.TextArea.Foreground = new SolidColorBrush(Colors.Gainsboro);
            scriptEditor.TextArea.TextView.LinkTextForegroundBrush = new SolidColorBrush(Colors.DodgerBlue);
            using (XmlReader reader = XmlReader.Create("LSHighlighting.xshd"))
            {
                scriptEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            }
        }

        private void openDocButton_Click(object sender, RoutedEventArgs e)
        {
            (new MainDialog(new DialogLSDoc(), "LoliScript Documentation")).Show();
        }
    }
}
