using OpenBullet.Plugins;
using System.Windows.Controls;

namespace OpenBullet.Views.UserControls
{
    /// <summary>
    /// Logica di interazione per UserControlDropdown.xaml
    /// </summary>
    public partial class UserControlDropdown : UserControl, IControl
    {
        public UserControlDropdown(string value, string[] options)
        {
            InitializeComponent();
            DataContext = this;

            foreach (var option in options)
            {
                Dropdown.Items.Add(option);
            }

            SetValue(value);
        }

        public dynamic GetValue()
        {
            return Dropdown.SelectedValue;
        }

        public void SetValue(dynamic value)
        {
            Dropdown.SelectedValue = (string)value;
        }
    }
}
