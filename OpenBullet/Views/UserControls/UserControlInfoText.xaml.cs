using OpenBullet.Plugins;
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

namespace OpenBullet.Views.UserControls
{
    /// <summary>
    /// Logica di interazione per UserControlInfoText.xaml
    /// </summary>
    public partial class UserControlInfoText : IControl
    {
        public string Value { get; set; }

        public UserControlInfoText(string defaultValue)
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
