using Microsoft.Win32;
using OpenBullet.Plugins;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet.Views.UserControls
{
    /// <summary>
    /// Logica di interazione per UserControlFilePicker.xaml
    /// </summary>
    public partial class UserControlFilePicker : UserControl, IControl
    {
        public string Filter { get; set; }

        public UserControlFilePicker(string location, string filter)
        {
            InitializeComponent();
            DataContext = this;

            Filter = filter;
            SetValue(location);
        }

        public dynamic GetValue()
        {
            return Location.Text;
        }

        public void SetValue(dynamic value)
        {
            Location.Text = (string)value;
        }

        private void Choose_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = Filter;

            var result = ofd.ShowDialog();

            if (result.HasValue && result.Value)
            {
                SetValue(ofd.FileName);
            }
        }
    }
}
