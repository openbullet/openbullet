using Microsoft.Win32;
using OpenBullet.Plugins;
using RuriLib.Models;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet.Views.UserControls
{
    /// <summary>
    /// Logica di interazione per UserControlWordlist.xaml
    /// </summary>
    public partial class UserControlWordlist : UserControl, IControl
    {
        public Wordlist Wordlist { get; set; } = null;

        public UserControlWordlist()
        {
            InitializeComponent();
            DataContext = this;
        }

        public dynamic GetValue()
        {
            return Wordlist;
        }

        public void SetValue(dynamic value)
        {
            Wordlist = (Wordlist)value;
        }

        private void Choose_Click(object sender, RoutedEventArgs e)
        {
            new MainDialog(new DialogSelectWordlist(this), "Select a Wordlist").ShowDialog();
            if (Wordlist != null)
            {
                WordlistName.Text = Wordlist.Name;
            }
        }
    }
}
