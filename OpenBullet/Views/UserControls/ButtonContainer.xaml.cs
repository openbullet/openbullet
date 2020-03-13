using System.Windows;
using System.Windows.Controls;

namespace OpenBullet.Views.UserControls
{
    /// <summary>
    /// Logica di interazione per ButtonContainer.xaml
    /// </summary>
    public partial class ButtonContainer : UserControl
    {
        public string Text { get; set; }
        private string MethodName { get; set; }
        private PluginControl PluginControl { get; set; }

        public ButtonContainer(string text, string methodName, PluginControl pluginControl)
        {
            InitializeComponent();
            DataContext = this;

            Text = text;
            MethodName = methodName;
            PluginControl = pluginControl;
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            PluginControl.RunMethod(MethodName);
        }
    }
}
