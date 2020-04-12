using Microsoft.Win32;
using OpenBullet.Views.Main;
using RuriLib.Models;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpenBullet
{
    /// <summary>
    /// Logica di interazione per DialogAddWordlist.xaml
    /// </summary>
    public partial class DialogAddWordlist : Page
    {
        public object Caller { get; set; }

        public DialogAddWordlist(object caller)
        {
            InitializeComponent();

            Caller = caller;

            foreach (string i in OB.Settings.Environment.GetWordlistTypeNames())
                typeCombobox.Items.Add(i);

            typeCombobox.SelectedIndex = 0;
        }
        
        private void acceptButton_Click(object sender, RoutedEventArgs e)
        {
            if (Caller.GetType() == typeof(WordlistManager))
            {
                if (nameTextbox.Text.Trim() == string.Empty) { MessageBox.Show("The name cannot be blank"); return; }

                var path = locationTextbox.Text;
                var cwd = Directory.GetCurrentDirectory();
                if (path.StartsWith(cwd)) path = path.Substring(cwd.Length + 1);
                ((WordlistManager)Caller).AddWordlist(new Wordlist(nameTextbox.Text, path, typeCombobox.Text, purposeTextbox.Text));
            }
            ((MainDialog)Parent).Close();
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Wordlist files | *.txt";
            ofd.FilterIndex = 1;
            ofd.ShowDialog();
            locationTextbox.Text = ofd.FileName;
            nameTextbox.Text = Path.GetFileNameWithoutExtension(ofd.FileName);

            // Set the recognized wordlist type
            try
            {
                var first = File.ReadLines(ofd.FileName).First();
                typeCombobox.Text = OB.Settings.Environment.RecognizeWordlistType(first);
            }
            catch { }
        }
    }
}
