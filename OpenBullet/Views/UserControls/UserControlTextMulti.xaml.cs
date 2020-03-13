using OpenBullet.Plugins;
using System;
using System.Windows.Controls;

namespace OpenBullet.Views.UserControls
{
    /// <summary>
    /// Logica di interazione per UserControlTextMulti.xaml
    /// </summary>
    public partial class UserControlTextMulti : UserControl, IControl
    {
        public UserControlTextMulti(string[] value)
        {
            InitializeComponent();
            DataContext = this;

            SetValue(value);
        }

        public dynamic GetValue()
        {
            return Box.Text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
        }

        public void SetValue(dynamic value)
        {
            var val = (string[])value;
            Box.Text = string.Join(Environment.NewLine, val);
        }
    }
}
