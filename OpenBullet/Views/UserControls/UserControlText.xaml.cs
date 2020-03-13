using OpenBullet.Plugins;

namespace OpenBullet.Views.UserControls
{
    /// <summary>
    /// Logica di interazione per UserControlText.xaml
    /// </summary>
    public partial class UserControlText : IControl
    {
        public string Value { get; set; }

        public UserControlText(string defaultValue)
        {
            InitializeComponent();
            DataContext = this;

            Value = defaultValue;
        }

        public dynamic GetValue()
        {
            return Value;
        }

        public void SetValue(dynamic value)
        {
            Value = (string)value;
        }
    }
}
