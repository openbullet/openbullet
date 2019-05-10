using OpenQA.Selenium.Remote;
using RuriLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuriLib.Runner
{
    /// <summary>
    /// Bot class. Includes a Worker that is used to perform checks on input data in the Runner.
    /// </summary>
    public class RunnerBotViewModel : ViewModelBase
    {
        private int id;
        /// <summary>The unique id of the bot (usually between 1 and the total amount of Bots).</summary>
        public int Id { get { return id; } set { id = value; OnPropertyChanged(); } }

        private string data = "";
        /// <summary>The data that needs to be checked.</summary>
        public string Data { get { return data; } set { data = value; OnPropertyChanged(); } }

        private string proxy = "";
        /// <summary>The proxy that the Worker is using to perform requests.</summary>
        public string Proxy { get { return proxy; } set { proxy = value; OnPropertyChanged(); } }

        private string status = "";
        /// <summary>The status of the bot.</summary>
        public string Status { get { return status; } set { status = value; OnPropertyChanged(); } }

        /// <summary>The Worker that performs the checks.</summary>
        public AbortableBackgroundWorker Worker { get; set; }

        /// <summary>The Selenium WebDriver.</summary>
        public RemoteWebDriver Driver { get; set; } = null;

        /// <summary>Whether the Selenium WebDriver is open or not.</summary>
        public bool IsDriverOpen { get; set; } = false;

        /// <summary>
        /// Creates an instance of a bot given an id.
        /// </summary>
        /// <param name="id">The unique id that will be assigned to the bot.</param>
        public RunnerBotViewModel(int id)
        {
            Id = id;
            Worker = new AbortableBackgroundWorker();
            Worker.WorkerSupportsCancellation = true;
            Worker.Id = id;
        }
    }
}
