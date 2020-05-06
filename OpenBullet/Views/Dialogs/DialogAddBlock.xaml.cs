using OpenBullet.Views.Main.Configs;
using RuriLib;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace OpenBullet.Views.Dialogs
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

            // Add rows to the grid
            for(var i = 0; i < OB.BlockPlugins.Count; i += 3)
            {
                pluginsGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            }

            // Create the buttons and fill the grid
            for(var i = 0; i < OB.BlockPlugins.Count; i++)
            {
                var plugin = OB.BlockPlugins[i];
                var button = new Button();
                button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(plugin.Color));
                if (plugin.LightForeground) button.Foreground = new SolidColorBrush(Colors.Gainsboro);
                button.Content = plugin.Name;
                button.SetValue(Grid.ColumnProperty, i % 3);
                button.SetValue(Grid.RowProperty, i / 3);
                button.Tag = plugin.GetType();
                button.Click += pluginButton_Click;
                pluginsGrid.Children.Add(button);
            }

            defaultSetLabel_MouseDown(this, null);
        }

        private void pluginButton_Click(object sender, RoutedEventArgs e)
        {
            var type = (e.OriginalSource as Button).Tag as Type;
            SendBack(Activator.CreateInstance(type) as BlockBase);
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

        private void blockSolveCaptchaButton_Click(object sender, RoutedEventArgs e)
        {
            SendBack(new BlockSolveCaptcha());
        }

        private void blockReportCaptchaButton_Click(object sender, RoutedEventArgs e)
        {
            SendBack(new BlockReportCaptcha());
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

        private void defaultSetLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            defaultSetLabel.Foreground = Utils.GetBrush("ForegroundMenuSelected");
            pluginsSetLabel.Foreground = Utils.GetBrush("ForegroundMain");
            blockSetTabControl.SelectedIndex = 0;
        }

        private void pluginsSetLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            defaultSetLabel.Foreground = Utils.GetBrush("ForegroundMain");
            pluginsSetLabel.Foreground = Utils.GetBrush("ForegroundMenuSelected");
            blockSetTabControl.SelectedIndex = 1;
        }
    }
}
