using OpenBullet.Views.UserControls;
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

namespace OpenBullet.Views.Main
{
    /// <summary>
    /// Logica di interazione per PluginsSection.xaml
    /// </summary>
    public partial class PluginsSection : Page
    {
        private List<PluginControl> controls;

        public PluginsSection(IEnumerable<PluginControl> controls)
        {
            InitializeComponent();
            this.controls = controls.ToList();

            foreach (var control in this.controls)
            {
                pluginSelector.Items.Add(control.Plugin.Name);
            }

            if (this.controls.Count > 0)
            {
                pluginSelector.SelectedIndex = 0;
            }
        }

        private void pluginSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Main.Content = controls.First(c => c.Plugin.Name == (string)pluginSelector.SelectedValue);
        }
    }
}
