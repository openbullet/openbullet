using OpenBullet.Plugins;
using System.Windows.Controls;

namespace OpenBullet.Views.UserControls
{
    /// <summary>
    /// Logica di interazione per UserControlCheckbox.xaml
    /// </summary>
    public partial class UserControlCheckbox : UserControl, IControl
    {
        public bool Value { get; set; }

        public UserControlCheckbox(bool value)
        {
            InitializeComponent();
            DataContext = this;

            Value = value;
        }

        public dynamic GetValue()
        {
            return Value;
        }

        public void SetValue(dynamic value)
        {
            Value = (bool)value;
        }
    }
}
