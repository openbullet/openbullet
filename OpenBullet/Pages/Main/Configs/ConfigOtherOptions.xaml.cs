using System.Windows.Controls;
using System.Windows.Input;

namespace OpenBullet
{
    /// <summary>
    /// Logica di interazione per ConfigOtherOptions.xaml
    /// </summary>
    public partial class ConfigOtherOptions : Page
    {
        public ConfigOtherOptionsGeneral GeneralPage = new ConfigOtherOptionsGeneral();
        public ConfigOtherOptionsRequests RequestsPage = new ConfigOtherOptionsRequests();
        public ConfigOtherOptionsProxies ProxiesPage = new ConfigOtherOptionsProxies();
        public ConfigOtherOptionsInputs InputsPage = new ConfigOtherOptionsInputs();
        public ConfigOtherOptionsData DataPage = new ConfigOtherOptionsData();
        public ConfigOtherOptionsSelenium SeleniumPage = new ConfigOtherOptionsSelenium();

        public ConfigOtherOptions()
        {
            InitializeComponent();

            menuOptionGeneral_MouseDown(this, null);
        }

        #region Menu Options
        private void menuOptionGeneral_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Main.Content = GeneralPage;
            menuOptionSelected(menuOptionGeneral);
        }

        private void menuOptionRequests_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Main.Content = RequestsPage;
            menuOptionSelected(menuOptionRequests);
        }

        private void menuOptionProxies_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Main.Content = ProxiesPage;
            menuOptionSelected(menuOptionProxies);
        }

        private void menuOptionInput_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Main.Content = InputsPage;
            menuOptionSelected(menuOptionInput);
        }

        private void menuOptionData_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Main.Content = DataPage;
            menuOptionSelected(menuOptionData);
        }

        private void menuOptionSelenium_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Main.Content = SeleniumPage;
            menuOptionSelected(menuOptionSelenium);
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
            ((Label)sender).Foreground = Globals.GetBrush("ForegroundGood");
        }


        #endregion
    }
}
