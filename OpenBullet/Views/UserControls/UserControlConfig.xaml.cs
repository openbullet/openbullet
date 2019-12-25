using Microsoft.Win32;
using OpenBullet.Plugins;
using RuriLib.Models;
using RuriLib.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet.Views.UserControls
{
    /// <summary>
    /// Logica di interazione per UserControlConfig.xaml
    /// </summary>
    public partial class UserControlConfig : UserControl, IControl
    {
        public ConfigViewModel Config { get; set; } = null;

        public UserControlConfig()
        {
            InitializeComponent();
            DataContext = this;
        }

        public dynamic GetValue()
        {
            return Config;
        }

        public void SetValue(dynamic value)
        {
            Config = (ConfigViewModel)value;
        }

        private void Choose_Click(object sender, RoutedEventArgs e)
        {
            new MainDialog(new DialogSelectConfig(this), "Select a Config").ShowDialog();
            if (Config != null)
            {
                ConfigName.Text = Config.Name;
            }
        }
    }
}
