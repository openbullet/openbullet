using OpenBullet.Plugins;
using OpenBullet.Views.UserControls;
using PluginFramework;
using RuriLib;
using RuriLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;

namespace OpenBullet.Views.StackerBlocks
{
    /// <summary>
    /// Logica di interazione per BlockPluginControl.xaml
    /// </summary>
    public partial class BlockPluginPage : Page
    {
        public IBlockPlugin BlockPlugin { get; set; }
        public BlockBase Block => BlockPlugin as BlockBase;
        public ObservableCollection<UserControl> Controls { get; set; } = new ObservableCollection<UserControl>();
        private List<PropertyInfo> ValidProperties { get; set; } = new List<PropertyInfo>();

        public BlockPluginPage(IBlockPlugin blockPlugin)
        {
            InitializeComponent();
            DataContext = this;

            BlockPlugin = blockPlugin;

            // For each valid property, add input field
            foreach (var p in BlockPlugin.GetType().GetProperties().Where(p => Check.InputProperty(p)))
            {
                ValidProperties.Add(p);
                Controls.Add(Build.InputField(BlockPlugin, p));
            }
        }

        public void SetPropertyValues()
        {
            foreach (var property in ValidProperties)
            {
                // Retrieve and set the value
                var value = Controls
                    .Where(c => c is UserControlContainer)
                    .Select(c => c as UserControlContainer)
                    .First(c => c.PropertyName == property.Name).GetValue();
                
                property.SetValue(BlockPlugin, value);
            }
        }
    }
}
