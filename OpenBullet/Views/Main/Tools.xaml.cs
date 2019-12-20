using OpenBullet.Views.Main.Tools;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpenBullet.Views.Main
{
    /// <summary>
    /// Logica di interazione per ToolsSection.xaml
    /// </summary>
    public partial class ToolsSection : Page
    {
        ListGenerator ListGenerator;
        SeleniumTools SeleniumTools;
        Database Database;

        public ToolsSection()
        {
            InitializeComponent();

            ListGenerator = new ListGenerator();
            SeleniumTools = new SeleniumTools();
            Database = new Database();

            menuOptionListGenerator_MouseDown(this, null);
        }

        #region Menu Options
        private void menuOptionListGenerator_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Main.Content = ListGenerator;
            menuOptionSelected(menuOptionListGenerator);
        }

        private void menuOptionSeleniumUtilities_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Main.Content = SeleniumTools;
            menuOptionSelected(menuOptionSeleniumTools);
        }

        private void MenuOptionDatabase_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Main.Content = Database;
            menuOptionSelected(menuOptionDatabase);
        }

        private void menuOptionSelected(object sender)
        {
            foreach (var child in topMenu.Children)
            {
                try
                {
                    var c = (Label)child;
                    c.Foreground = Globals.GetBrush("ForegroundMain");
                }
                catch { }
            }
            ((Label)sender).Foreground = Globals.GetBrush("ForegroundCustom");
        }
        #endregion
    }
}
