using OpenBullet.Pages.StackerBlocks;
using RuriLib;
using RuriLib.ViewModels;
using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace OpenBullet.ViewModels
{
    public class StackerBlockViewModel : ViewModelBase
    {
        private int id;
        public int Id { get { return id; } set { id = value; OnPropertyChanged("Id"); } }

        private bool selected = false;
        public bool Selected { get { return selected; } set { selected = value; OnPropertyChanged("Selected"); OnPropertyChanged("BorderColor"); } }
        public SolidColorBrush BorderColor { get { return new SolidColorBrush(Selected ? Colors.White : Colors.Black); } }

        public SolidColorBrush Color { get { return new SolidColorBrush(Block.Disabled ? Colors.Gray : GetBlockColor()); } }
        public SolidColorBrush Foreground { get { return new SolidColorBrush(Block.IsSelenium ? Colors.White : Colors.Black); } }

        private int height;
        public int Height { get { return height; } set { height = value; OnPropertyChanged("Height"); OnPropertyChanged("FontSize"); } }
        public int FontSize { get { return (int)(Height / 3); } }

        private Page page;
        public Page Page { get { return page; } set { page = value; OnPropertyChanged("Page"); } }

        private BlockBase block;
        public BlockBase Block { get { return block; } set { block = value; OnPropertyChanged("Block"); } }

        public void Disable()
        {
            if (block.GetType() == typeof(BlockLSCode)) return;
            Block.Disabled = !Block.Disabled;
            OnPropertyChanged("Color");
        }

        public void UpdateHeight(int height)
        {
            Height = height;
            OnPropertyChanged("Height");
            OnPropertyChanged("FontSize");
        }

        public StackerBlockViewModel(BlockBase block, Random rand)
        {
            Id = rand.Next();
            Block = block;
            OnPropertyChanged("Label");
            OnPropertyChanged("Color");

            // Define mapping scheme
            var ts = new TypeSwitch()
                .Case((BlockParse x) => Page = new PageBlockParse(x))
                .Case((BlockKeycheck x) => Page = new PageBlockKeycheck(x))
                .Case((BlockRequest x) => Page = new PageBlockRequest(x))
                .Case((BlockRecaptcha x) => Page = new PageBlockRecaptcha(x))
                .Case((BlockFunction x) => Page = new PageBlockFunction(x))
                .Case((BlockImageCaptcha x) => Page = new PageBlockCaptcha(x))
                .Case((BlockBypassCF x) => Page = new PageBlockBypassCF(x))
                .Case((BlockUtility x) => Page = new PageBlockUtility(x))
                .Case((BlockLSCode x) => Page = new PageBlockLSCode(x))
                .Case((BlockTCP x) => Page = new PageBlockTCP(x))
                .Case((SBlockNavigate x) => Page = new PageSBlockNavigate(x))
                .Case((SBlockBrowserAction x) => Page = new PageSBlockBrowserAction(x))
                .Case((SBlockElementAction x) => Page = new PageSBlockElementAction(x))
                .Case((SBlockExecuteJS x) => Page = new PageSBlockExecuteJS(x));

            ts.Switch(Block);
        }

        
        public Color GetBlockColor()
        {
            Color color = Colors.Black;

            // Define mapping scheme
            var ts = new TypeSwitch()
                .Case((BlockParse x) => color = Colors.Gold)
                .Case((BlockKeycheck x) => color = Colors.DodgerBlue)
                .Case((BlockRequest x) => color = Colors.LimeGreen)
                .Case((BlockRecaptcha x) => color = Colors.Turquoise)
                .Case((BlockFunction x) => color = Colors.YellowGreen)
                .Case((BlockImageCaptcha x) => color = Colors.DarkOrange)
                .Case((BlockBypassCF x) => color = Colors.DarkSalmon)
                .Case((BlockUtility x) => color = Colors.Wheat)
                .Case((BlockLSCode x) => color = Colors.White)
                .Case((BlockTCP x) => color = Colors.MediumPurple)
                .Case((SBlockNavigate x) => color = Colors.RoyalBlue)
                .Case((SBlockBrowserAction x) => color = Colors.Green)
                .Case((SBlockElementAction x) => color = Colors.Firebrick)
                .Case((SBlockExecuteJS x) => color = Colors.Indigo);

            // Execute type check to set color data
            ts.Switch(Block);

            return color;
        }
    }
}
