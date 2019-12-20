using RuriLib;
using System.Windows.Controls;

namespace OpenBullet.Pages.StackerBlocks
{
    /// <summary>
    /// Logica di interazione per PageSBlockNavigate.xaml
    /// </summary>
    public partial class PageSBlockNavigate : Page
    {
        SBlockNavigate vm;

        public PageSBlockNavigate(SBlockNavigate block)
        {
            InitializeComponent();
            vm = block;
            DataContext = vm;
        }
    }
}
