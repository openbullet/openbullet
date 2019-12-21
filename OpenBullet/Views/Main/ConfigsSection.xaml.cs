using OpenBullet.Views.Main.Configs;
using RuriLib.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpenBullet.Views.Main
{
    /// <summary>
    /// Logica di interazione per Configs.xaml
    /// </summary>
    public partial class ConfigsSection : Page
    {
        public ConfigManager ConfigManagerPage;
        public Stacker StackerPage;
        public ConfigOtherOptions OtherOptionsPage;
        public ConfigViewModel CurrentConfig { get; set; }

        public ConfigsSection()
        {
            InitializeComponent();

            ConfigManagerPage = new ConfigManager();
            Globals.logger.LogInfo(Components.ConfigManager, "Initialized Manager Page");

            menuOptionManager_MouseDown(this, null);
        }

        #region Menu Options
        private void menuOptionManager_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Main.Content = ConfigManagerPage;
            menuOptionSelected(menuOptionManager);
        }

        public void menuOptionStacker_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(CurrentConfig != null && StackerPage != null)
            {
                Main.Content = StackerPage;
                menuOptionSelected(menuOptionStacker);
            }
            else
            {
                Globals.logger.LogError(Components.ConfigManager, "Cannot switch to stacker since no config is loaded or the loaded config isn't public");
            }
        }

        private void menuOptionOtherOptions_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(CurrentConfig != null)
            {
                if (OtherOptionsPage == null)
                    OtherOptionsPage = new ConfigOtherOptions();    
                
                Main.Content = OtherOptionsPage;
                menuOptionSelected(menuOptionOtherOptions);
            }
            else
            {
                Globals.logger.LogError(Components.ConfigManager, "Cannot switch to other options since no config is loaded");
            }
        }

        private void menuOptionSelected(object sender)
        {
            foreach (var child in topMenu.Children)
            {
                try
                {
                    //var a = "";
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
