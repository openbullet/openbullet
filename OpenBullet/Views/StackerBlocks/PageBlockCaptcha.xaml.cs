using RuriLib;
using System.Windows.Controls;

namespace OpenBullet.Pages.StackerBlocks
{
    /// <summary>
    /// Logica di interazione per PageBlockCaptcha.xaml
    /// </summary>
    public partial class PageBlockCaptcha : Page
    {
        BlockImageCaptcha vm;

        public PageBlockCaptcha(BlockImageCaptcha block)
        {
            InitializeComponent();
            vm = block;
            DataContext = vm;
        }
    }
}
