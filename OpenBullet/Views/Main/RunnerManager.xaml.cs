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
        private RunnerManagerViewModel vm = null;
        private bool DelegateCalled { get; set; } = false;

        public delegate void StartRunnerEventHandler(object sender, EventArgs e);
        public event StartRunnerEventHandler StartRunner;
        protected virtual void OnStartRunner()
        {
            StartRunner?.Invoke(this, EventArgs.Empty);
        }

        public RunnerManager()
        {
            vm = OB.RunnerManager;
            DataContext = vm;

            InitializeComponent();
        }

        #region Buttons
        private void addRunnerButton_Click(object sender, RoutedEventArgs e)
        {
            vm.Create();
        }

        private void removeRunnerButton_Click(object sender, RoutedEventArgs e)
        {
            var id = (int)((Button)e.OriginalSource).Tag;
            if (vm.Get(id).ViewModel.Master.Status != WorkerStatus.Idle)
            {
                MessageBox.Show("The Runner is active! Please stop it before removing it.");
                return;
            }
            vm.Remove(id);
        }

        private void startRunnerButton_Click(object sender, RoutedEventArgs e)
        {
            var id = (int)((Button)e.OriginalSource).Tag;
            var runner = vm.Get(id);

            StartRunner += runner.View.OnStartRunner;
            OnStartRunner();
            StartRunner -= runner.View.OnStartRunner;
        }
        #endregion

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
                OB.MainWindow.ShowRunner(vm.Get(id).View);
            }
        }

        #region Quick Access Setters
        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            T parent = parentObject as T;
            if (parent != null) return parent;
            else return FindParent<T>(parentObject);
        }

        private void selectConfig_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var id = (int)(FindParent<Grid>(sender as DependencyObject)).Tag;
            var runner = OB.MainWindow.RunnerManagerPage.vm.Get(id);

            if (!runner.ViewModel.Busy)
            {
                DelegateCalled = true;
                (new MainDialog(new DialogSelectConfig(runner.View), "Select Config")).ShowDialog();
            }
        }

        private void selectWordlist_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var id = (int)(FindParent<Grid>(sender as DependencyObject)).Tag;
            var runner = OB.MainWindow.RunnerManagerPage.vm.Get(id);

            if (!runner.ViewModel.Busy)
            {
                DelegateCalled = true;
                (new MainDialog(new DialogSelectWordlist(runner.View), "Select Wordlist")).ShowDialog();
            }
        }

        private void selectProxies_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var id = (int)(FindParent<Grid>(sender as DependencyObject)).Tag;
            var runner = OB.MainWindow.RunnerManagerPage.vm.Get(id);

            if (!runner.ViewModel.Busy)
            {
                DelegateCalled = true;
                (new MainDialog(new DialogSetProxies(runner.ViewModel), "Set Proxies")).ShowDialog();
            }
        }

        private void selectBots_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var id = (int)(FindParent<Grid>(sender as DependencyObject)).Tag;
            var runner = OB.MainWindow.RunnerManagerPage.vm.Get(id);

            if (!runner.ViewModel.Busy)
            {
                DelegateCalled = true;
                (new MainDialog(new DialogSelectBots(runner.ViewModel, runner.ViewModel.BotsAmount), "Select Bots Number")).ShowDialog();
            }
        }
        #endregion

        private void stopAllRunnersButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var runner in vm.RunnersCollection.Where(r => r.ViewModel.Busy))
            {
                StartRunner += runner.View.OnStartRunner;
                OnStartRunner();
                StartRunner -= runner.View.OnStartRunner;
            }
        }

        private void removeAllRunnersButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show($"Are you sure you want to remove all Runners?", 
                "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                var list = vm.RunnersCollection.Where(r => !r.ViewModel.Busy).ToList();
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    vm.RunnersCollection.Remove(list[i]);
                }
            }
        }

        private void startAllRunnersButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var runner in vm.RunnersCollection.Where(r => !r.ViewModel.Busy))
            {
                StartRunner += runner.View.OnStartRunner;
                OnStartRunner();
                StartRunner -= runner.View.OnStartRunner;
            }
        }
    }
}
