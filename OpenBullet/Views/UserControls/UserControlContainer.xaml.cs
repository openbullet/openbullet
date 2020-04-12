using OpenBullet.Plugins;
using System.Windows.Controls;

namespace OpenBullet.Views.UserControls
{
    /// <summary>
    /// Logica di interazione per UserControlContainer.xaml
    /// </summary>
    public partial class UserControlContainer : UserControl, IControl
    {
        public string PropertyName { get; set; }
        public IControl UserControl { get; set; }

        public string Label { get; set; }
        public string Tooltip { get; set; }

        public UserControlContainer(string propertyName, IControl userControl, string label, string tooltip)
        {
            InitializeComponent();
            DataContext = this;

            PropertyName = propertyName;
            UserControl = userControl;

            var uc = UserControl as UserControl;
            uc.SetValue(Grid.ColumnProperty, 1);
            Grid.Children.Add(UserControl as UserControl);

            Label = label;
            Tooltip = tooltip;
        }

        public dynamic GetValue()
        {
            return UserControl.GetValue();
        }

        public void SetValue(dynamic value)
        {
            UserControl.SetValue(value);
        }
    }
}
