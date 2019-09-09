using System.Windows.Controls;
using System.Windows.Input;

namespace OpenBullet
{
    /// <summary>
    /// Logica di interazione per Tools.xaml
    /// </summary>
    public partial class Tools : Page
    {
        ToolsListGenerator ListGenerator;
        ToolsSeleniumTools SeleniumTools;
        ToolsDatabase Database;

        public Tools()
        {
            InitializeComponent();

            ListGenerator = new ToolsListGenerator();
            SeleniumTools = new ToolsSeleniumTools();
            Database = new ToolsDatabase();

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
