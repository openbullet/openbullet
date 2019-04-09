using Microsoft.Win32;
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

            foreach (string i in Globals.environment.GetWordlistTypeNames())
                typeCombobox.Items.Add(i);

            typeCombobox.SelectedIndex = 0;
        }
        
        private void acceptButton_Click(object sender, RoutedEventArgs e)
        {
            if (Caller.GetType() == typeof(WordlistManager))
            {
                if (nameTextbox.Text.Trim() == "") { MessageBox.Show("The name cannot be blank"); return; }
                ((WordlistManager)Caller).AddWordlist(new Wordlist(nameTextbox.Text, locationTextbox.Text, typeCombobox.Text, purposeTextbox.Text));
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
                typeCombobox.Text = Globals.environment.RecognizeWordlistType(first);
            }
            catch { }
        }
    }
}
