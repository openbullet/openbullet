using RuriLib;
using System.Windows.Controls;

namespace OpenBullet.Pages.StackerBlocks
{
    /// <summary>
    /// Logica di interazione per PageBlockBypassCF.xaml
    /// </summary>
    public partial class PageBlockBypassCF : Page
    {
        BlockBypassCF vm;

        public PageBlockBypassCF(BlockBypassCF block)
        {
            InitializeComponent();
            vm = block;
            DataContext = vm;
        }
    }
}
