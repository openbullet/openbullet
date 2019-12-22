using OpenBullet.Views.StackerBlocks;
using PluginFramework;
using RuriLib;
using RuriLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;

namespace OpenBullet.ViewModels
{
    public class StackerBlockViewModel : ViewModelBase
    {
        private int id;
        public int Id { get => id; set { id = value; OnPropertyChanged(); } }

        private bool selected = false;
        public bool Selected { get => selected; set { selected = value; OnPropertyChanged(); OnPropertyChanged(nameof(BorderColor)); } }
        public SolidColorBrush BorderColor => new SolidColorBrush(Selected ? Colors.White : Colors.Black);

        public SolidColorBrush Color => new SolidColorBrush(Block.Disabled ? Colors.Gray : GetBlockColor()); 
        public SolidColorBrush Foreground => new SolidColorBrush(Block.IsSelenium ? Colors.White : Colors.Black);

        private int height;
        public int Height { get => height; set { height = value; OnPropertyChanged(); OnPropertyChanged(nameof(FontSize)); } }
        public int FontSize => Height / 3;

        private Page page;
        public Page Page { get => page; set { page = value; OnPropertyChanged(); } }

        private BlockBase block;
        public BlockBase Block { get => block; set { block = value; OnPropertyChanged(); } }

        public void Disable()
        {
            if (block.GetType() == typeof(BlockLSCode)) return;
            Block.Disabled = !Block.Disabled;
            OnPropertyChanged(nameof(Color));
        }

        public void UpdateHeight(int height)
        {
            Height = height;
            OnPropertyChanged(nameof(Height));
            OnPropertyChanged(nameof(FontSize));
        }

        public StackerBlockViewModel(BlockBase block, Random rand)
        {
            Id = rand.Next();
            Block = block;
            OnPropertyChanged(nameof(Block.Label));
            OnPropertyChanged(nameof(Color));

            // Initialize the page
            if (OB.BlockMappings.Any(m => m.Item1 == block.GetType()))
            {
                Page = Activator.CreateInstance(
                    OB.BlockMappings.First(m => m.Item1 == block.GetType()).Item2,
                    new object[] { block }) as Page;
            }
            else
            {
                throw new Exception($"Tried to initialize a page for the block type {block.GetType().Name} but it wasn't found in the mappings");
            }
        }

        
        public Color GetBlockColor()
        {
            return OB.BlockMappings.First(m => m.Item1 == block.GetType()).Item3;
        }
    }
}
