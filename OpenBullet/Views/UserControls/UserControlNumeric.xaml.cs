using OpenBullet.Plugins;
using System.Windows.Controls;

namespace OpenBullet.Views.UserControls
{
    /// <summary>
    /// Logica di interazione per UserControlNumeric.xaml
    /// </summary>
    public partial class UserControlNumeric : UserControl, IControl
    {
        public int Value { get; set; }
        public int Minimum { get; set; }
        public int Maximum { get; set; }

        public UserControlNumeric(int defaultValue, int minimum, int maximum)
        {
            InitializeComponent();
            DataContext = this;

            Value = defaultValue;
            Minimum = minimum;
            Maximum = maximum;
        }

        public dynamic GetValue()
        {
            return Value;
        }

        public void SetValue(dynamic value)
        {
            Value = (int)value;
        }
    }
}
