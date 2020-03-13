using OpenBullet.Views.Main.Runner;
using RuriLib.Interfaces;
using RuriLib.Runner;
using RuriLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace OpenBullet.ViewModels
{
    public class RunnerManagerViewModel : ViewModelBase, IRunnerManager
    {
        public ObservableCollection<RunnerInstance> RunnersCollection { get; set; } = new ObservableCollection<RunnerInstance>();

        public IEnumerable<IRunner> Runners => RunnersCollection.Select(i => i.ViewModel);

        private Random rand = new Random();

        public RunnerInstance Get(int id)
        {
            return RunnersCollection.Where(r => r.Id == id).First();
        }

        public IRunner Create()
        {
            var instance = new RunnerInstance(rand.Next());
            RunnersCollection.Add(instance);
            return instance.ViewModel;
        }

        public void Remove(IRunner runner)
        {
            RunnersCollection.Remove(RunnersCollection.First(r => r.ViewModel == runner));
        }

        public void Remove(int id)
        {
            RunnersCollection.Remove(Get(id));
        }

        public void RemoveAll()
        {
            RunnersCollection.Clear();
        }
    }

    public class RunnerInstance
    {
        public Runner View { get; private set; }
        public RunnerViewModel ViewModel { get; private set; }

        public int Id { get; set; }

        public RunnerInstance(int id)
        {
            Id = id;
            ViewModel = new RunnerViewModel(OB.Settings.Environment, OB.Settings.RLSettings);
            View = new Runner(ViewModel);
        }
    }
}
