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

        public IEnumerable<IRunner> Runners => RunnersCollection.Select(i => i.Runner);

        private Random rand = new Random();

        public RunnerInstance Get(int id)
        {
            return RunnersCollection.Where(r => r.Id == id).First();
        }

        public IRunner Create()
        {
            var instance = new RunnerInstance(rand.Next());
            RunnersCollection.Add(instance);
            return instance.Runner;
        }

        public void Remove(IRunner runner)
        {
            RunnersCollection.Remove(RunnersCollection.First(r => r.Runner == runner));
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
        public Runner Page { get; private set; }
        public RunnerViewModel Runner => Page.vm;

        public int Id { get; set; }

        public RunnerInstance(int id)
        {
            Id = id;
            Page = new Runner();
        }
    }
}
