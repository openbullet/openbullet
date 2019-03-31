using RuriLib;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet
{
    /// <summary>
    /// Logica di interazione per DialogAddBlock.xaml
    /// </summary>
    public partial class DialogAddBlock : Page
    {
        public object Caller { get; set; }

        public DialogAddBlock(object caller)
        {
            InitializeComponent();
            Caller = caller;
        }

        private void blockRequestButton_Click(object sender, RoutedEventArgs e)
        {
            SendBack(new BlockRequest());
        }

        private void blockUtilityButton_Click(object sender, RoutedEventArgs e)
        {
            SendBack(new BlockUtility());
        }

        private void blockKeycheckButton_Click(object sender, RoutedEventArgs e)
        {
            SendBack(new BlockKeycheck());
        }

        private void blockParseButton_Click(object sender, RoutedEventArgs e)
        {
            SendBack(new BlockParse());
        }

        private void blockFunctionButton_Click(object sender, RoutedEventArgs e)
        {
            SendBack(new BlockFunction());
        }

        private void blockRecaptchaButton_Click(object sender, RoutedEventArgs e)
        {
            SendBack(new BlockRecaptcha());
        }

        private void blockCaptchaButton_Click(object sender, RoutedEventArgs e)
        {
            SendBack(new BlockImageCaptcha());
        }

        private void blockBypassCFButton_Click(object sender, RoutedEventArgs e)
        {
            SendBack(new BlockBypassCF());
        }

        private void blockTCPButton_Click(object sender, RoutedEventArgs e)
        {
            SendBack(new BlockTCP());
        }
        
        private void blockNavigateButton_Click(object sender, RoutedEventArgs e)
        {
            SendBack(new SBlockNavigate());
        }

        private void blockBrowserActionButton_Click(object sender, RoutedEventArgs e)
        {
            SendBack(new SBlockBrowserAction());
        }

        private void blockElementActionButton_Click(object sender, RoutedEventArgs e)
        {
            SendBack(new SBlockElementAction());
        }

        private void blockExecuteJSButton_Click(object sender, RoutedEventArgs e)
        {
            SendBack(new SBlockExecuteJS());
        }

        private void SendBack(BlockBase block)
        {
            if (Caller.GetType() == typeof(Stacker))
            {
                ((Stacker)Caller).AddBlock(block);
            }
            ((MainDialog)Parent).Close();
        }
    }
}
