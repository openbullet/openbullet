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

namespace OpenBullet.Pages.StackerBlocks
{
    /// <summary>
    /// Logica di interazione per PageBlockTCP.xaml
    /// </summary>
    public partial class PageBlockTCP : Page
    {
        BlockTCP block;

        public PageBlockTCP(BlockTCP block)
        {
            InitializeComponent();
            this.block = block;
            DataContext = this.block;

            foreach (var c in Enum.GetNames(typeof(TCPCommand)))
                tcpCommandCombobox.Items.Add(c);

            tcpCommandCombobox.SelectedIndex = (int)block.TCPCommand;
        }

        private void tcpCommandCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            block.TCPCommand = (TCPCommand)((ComboBox)e.OriginalSource).SelectedIndex;

            switch (block.TCPCommand)
            {
                default:
                    commandTabControl.SelectedIndex = 0;
                    break;

                case TCPCommand.Connect:
                    commandTabControl.SelectedIndex = 1;
                    break;

                case TCPCommand.Send:
                    commandTabControl.SelectedIndex = 2;
                    break;
            }
        }
    }
}
