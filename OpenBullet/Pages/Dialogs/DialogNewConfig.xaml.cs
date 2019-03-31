using RuriLib;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpenBullet
{
    /// <summary>
    /// Logica di interazione per DialogNewConfig.xaml
    /// </summary>
    public partial class DialogNewConfig : Page
    {
        public object Caller { get; set; }

        public DialogNewConfig(object caller)
        {
            InitializeComponent();
            Caller = caller;
            authorTextbox.Text = Globals.obSettings.General.DefaultAuthor;
            nameTextbox.Focus();

            categoryCombobox.Items.Add("Default");
            foreach(var category in Globals.mainWindow.ConfigsPage.ConfigManagerPage.vm.ConfigsList.Select(c => c.Category).Distinct())
                categoryCombobox.Items.Add(category);
            
            categoryCombobox.SelectedIndex = 0;
        }

        private void acceptButton_Click(object sender, RoutedEventArgs e)
        {
            if (Caller.GetType() == typeof(ConfigManager))
            {
                // Check if name is ok
                if (nameTextbox.Text.Trim() == "") { MessageBox.Show("The name cannot be blank"); return; }
                else if (nameTextbox.Text != BlockBase.MakeValidFileName(nameTextbox.Text)) { MessageBox.Show("The name contains invalid characters"); return; }

                // Check if category is ok
                if (categoryCombobox.Text.Trim() == "") categoryCombobox.Text = "Default";
                else if (categoryCombobox.Text != BlockBase.MakeValidFileName(categoryCombobox.Text)) { MessageBox.Show("The category contains invalid characters"); return; }

                ((ConfigManager)Caller).CreateConfig(nameTextbox.Text, categoryCombobox.Text, authorTextbox.Text);
            }
            ((MainDialog)Parent).Close();
        }

        private void textbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                acceptButton_Click(this, new RoutedEventArgs());
        }
    }
}
