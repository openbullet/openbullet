using System;
using System.Windows;
using System.Windows.Input;

namespace OpenBullet
{
    /// <summary>
    /// Logica di interazione per SplashWindow.xaml
    /// </summary>
    public partial class SplashWindow : Window
    {
        public SplashWindow()
        { 
            InitializeComponent();
        }

        private void agreeButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void WindowMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void quitImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
