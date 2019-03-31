using System.Windows;
using System.Windows.Controls;

namespace OpenBullet
{
    /// <summary>
    /// Logica di interazione per MainDialog.xaml
    /// </summary>
    public partial class MainDialog : Window
    {
        public MainDialog(Page content, string title)
        {
            InitializeComponent();

            Content = content;
            Title = title;
        }
    }
}
