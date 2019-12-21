using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OpenBullet.Views.Main.Tools
{
    /// <summary>
    /// Logica di interazione per Database.xaml
    /// </summary>
    public partial class Database : Page
    {
        public Database()
        {
            InitializeComponent();
        }

        private void shrinkButton_Click(object sender, RoutedEventArgs e)
        {
            if (Globals.mainWindow.RunnerManagerPage.vm.Runners.Any(r => r.Runner.Master.IsBusy))
            {
                Globals.logger.LogWarning(Components.Database, "Please stop all active runners before shrinking the database!", true);
                return;
            }

            try
            {
                using (var db = new LiteDatabase(Globals.dataBaseFile))
                {
                    var previousSize = (int)(new FileInfo(Globals.dataBaseFile).Length / 1000);
                    db.Shrink();
                    var newSize = (int)(new FileInfo(Globals.dataBaseFile).Length / 1000);
                    Globals.logger.LogInfo(Components.Database, $"Database successfully shrinked from {previousSize} KB to {newSize} KB", true);
                }
            }
            catch (Exception ex)
            {
                Globals.logger.LogError(Components.Database, $"Shrink failed! Error: {ex.Message}");
            }
        }
    }
}
