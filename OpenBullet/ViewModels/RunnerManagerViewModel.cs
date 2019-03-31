using RuriLib.Runner;
using RuriLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace OpenBullet.ViewModels
{
    public class RunnerManagerViewModel : ViewModelBase
    {
        public ObservableCollection<RunnerInstanceViewModel> Runners { get; set; } = new ObservableCollection<RunnerInstanceViewModel>();
        private Random rand = new Random();
        
        public void CreateRunner() {
            Runners.Add(new RunnerInstanceViewModel() { Id = rand.Next() });
        }

        public RunnerInstanceViewModel GetRunnerById(int id)
        {
            return Runners.Where(r => r.Id == id).First();
        }

        public void RemoveRunnerById(int id)
        {
            Runners.Remove(GetRunnerById(id));
        }
    }

    public class RunnerInstanceViewModel : ViewModelBase
    {
        public Runner Page { get; set; } = new Runner();
        public RunnerViewModel Runner { get { return Page.vm; } }

        private int id;
        public int Id { get { return id; } set { id = value; OnPropertyChanged(); } }
    }
}
