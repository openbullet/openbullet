using OpenBullet.ViewModels;
using RuriLib.Runner;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace OpenBullet.Views.Main
{
    /// <summary>
    /// Logica di interazione per RunnerManager.xaml
    /// </summary>
    public partial class RunnerManager : Page
    {
        public RunnerManagerViewModel vm = new RunnerManagerViewModel();
        private bool DelegateCalled { get; set; } = false;

        public delegate void StartRunnerEventHandler(object sender, EventArgs e);
        public event StartRunnerEventHandler StartRunner;
        protected virtual void OnStartRunner()
        {
            StartRunner?.Invoke(this, EventArgs.Empty);
        }

        public RunnerManager(bool createFirst)
        {
            InitializeComponent();
            DataContext = vm;

            if (createFirst)
                addRunnerButton_Click(this, null);
        }

        private void addRunnerButton_Click(object sender, RoutedEventArgs e)
        {
            vm.CreateRunner();
            helpMessageLabel.Visibility = Visibility.Collapsed;
        }

        private void removeRunnerButton_Click(object sender, RoutedEventArgs e)
        {
            var id = (int)((Button)e.OriginalSource).Tag;
            if (vm.GetRunnerById(id).Runner.Master.Status != WorkerStatus.Idle)
            {
                MessageBox.Show("The Runner is active! Please stop it before removing it.");
                return;
            }
            vm.RemoveRunnerById(id);
        }

        private void startRunnerButton_Click(object sender, RoutedEventArgs e)
        {
            var id = (int)((Button)e.OriginalSource).Tag;
            var runner = vm.GetRunnerById(id);

            StartRunner += runner.Page.OnStartRunner;
            OnStartRunner();
            StartRunner -= runner.Page.OnStartRunner;
        }

        private void runnerInstanceGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DelegateCalled)
            {
                DelegateCalled = false;
                return;
            }

            if (sender.GetType() == typeof(Grid))
            {
                var id = (int)(sender as Grid).Tag;
                Globals.mainWindow.ShowRunner(vm.GetRunnerById(id).Page);
            }
        }

        #region Quick Access Setters
        private void selectConfig_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var id = (int)(FindParent<Grid>(sender as DependencyObject)).Tag;
            var runner = Globals.mainWindow.RunnerManagerPage.vm.GetRunnerById(id);

            if (!runner.Runner.Busy)
            {
                DelegateCalled = true;
                (new MainDialog(new DialogSelectConfig(runner.Page), "Select Config")).ShowDialog();
            }
        }

        private void selectWordlist_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var id = (int)(FindParent<Grid>(sender as DependencyObject)).Tag;
            var runner = Globals.mainWindow.RunnerManagerPage.vm.GetRunnerById(id);

            if (!runner.Runner.Busy)
            {
                DelegateCalled = true;
                (new MainDialog(new DialogSelectWordlist(runner.Page), "Select Wordlist")).ShowDialog();
            }
        }

        private void selectProxies_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var id = (int)(FindParent<Grid>(sender as DependencyObject)).Tag;
            var runner = Globals.mainWindow.RunnerManagerPage.vm.GetRunnerById(id);

            if (!runner.Runner.Busy)
            {
                DelegateCalled = true;
                (new MainDialog(new DialogSetProxies(runner.Page), "Set Proxies")).ShowDialog();
            }
        }

        private void selectBots_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var id = (int)(FindParent<Grid>(sender as DependencyObject)).Tag;
            var runner = Globals.mainWindow.RunnerManagerPage.vm.GetRunnerById(id);

            if (!runner.Runner.Busy)
            {
                DelegateCalled = true;
                (new MainDialog(new DialogSelectBots(runner.Page, runner.Runner.BotsAmount), "Select Bots Number")).ShowDialog();
            }
        }
        #endregion

        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            T parent = parentObject as T;
            if (parent != null) return parent;
            else return FindParent<T>(parentObject);
        }

        private void stopAllRunnersButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var runner in vm.Runners.Where(r => r.Runner.Busy))
            {
                StartRunner += runner.Page.OnStartRunner;
                OnStartRunner();
                StartRunner -= runner.Page.OnStartRunner;
            }
        }

        private void removeAllRunnersButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show($"Are you sure you want to remove all Runners?", 
                "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                var list = vm.Runners.Where(r => !r.Runner.Busy).ToList();
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    vm.Runners.Remove(list[i]);
                }
            }
        }

        private void startAllRunnersButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var runner in vm.Runners.Where(r => !r.Runner.Busy))
            {
                StartRunner += runner.Page.OnStartRunner;
                OnStartRunner();
                StartRunner -= runner.Page.OnStartRunner;
            }
        }
    }
}
