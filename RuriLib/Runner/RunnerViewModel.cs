﻿using Extreme.Net;
using Newtonsoft.Json;
using RuriLib.Interfaces;
using RuriLib.LS;
using RuriLib.Models;
using RuriLib.Models.Stats;
using RuriLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RuriLib.Runner
{
    /// <summary>
    /// Whether to use proxies or not for the current session.
    /// </summary>
    public enum ProxyMode
    {
        /// <summary>Use the default setting in the config.</summary>
        Default,
        /// <summary>Always use proxies.</summary>
        On,
        /// <summary>Never use proxies.</summary>
        Off
    }

    /// <summary>
    /// Main class that handles all the multi-threaded checking of a Wordlist given a Config.
    /// </summary>
    // TODO: Split this into partial classes
    public class RunnerViewModel : ViewModelBase, IRunnerMessaging, IRunner
    {
        #region Constructor
        /// <summary>
        /// Constructs the RunnerViewModel instance.
        /// </summary>
        /// <param name="environment">The environment settings</param>
        /// <param name="settings">The RuriLib settings</param>
        /// <param name="random">A reference to the global random generator</param>
        public RunnerViewModel(EnvironmentSettings environment, RLSettingsViewModel settings, Random random = null)
        {
            Env = environment;
            Settings = settings;
            this.random = random != null ? random : new Random();
            OnPropertyChanged("Busy");
            OnPropertyChanged("ControlsEnabled");
        }
        #endregion
        
        #region Settings
        private RLSettingsViewModel Settings { get; set; }
        private EnvironmentSettings Env { get; set; }
        private Random random;
        private object randomLocker = new object();
        #endregion

        #region Workers
        /// <summary>The Master Worker that manages all the other Workers and updates the observable properties.</summary>
        public AbortableBackgroundWorker Master { get; set; } = new AbortableBackgroundWorker();
        /// <summary>The status of the Master Worker.</summary>
        public WorkerStatus WorkerStatus { get { return Master.Status; } }
        /// <summary>The managed workers that run single data checks.</summary>
        public ObservableCollection<RunnerBotViewModel> Bots { get; } = new ObservableCollection<RunnerBotViewModel>();
        #endregion

        #region Visible Properties and Components
        /// <summary>Whether the Master Worker is busy or idle.</summary>
        public bool Busy => Master.Status != WorkerStatus.Idle;

        /// <summary>Whether the user can set the properties (a.k.a. whether the Master Worker is idle).</summary>
        public bool ControlsEnabled => !Busy;

        private int botsAmount = 1;
        /// <summary>The amount of bots to run simultaneously for multi-threaded checking.</summary>
        public int BotsAmount { get => botsAmount; set { botsAmount = value; OnPropertyChanged(); } }

        private int startingPoint = 1;
        /// <summary>How many data lines to skip before starting the checking process.</summary>
        public int StartingPoint { get { return startingPoint; } set { startingPoint = value; OnPropertyChanged(); OnPropertyChanged("Progress"); } }

        /// <summary>The amount of data lines checked, including the starting point.</summary>
        public int ProgressCount { get { return TestedCount + StartingPoint - 1; } }

        /// <summary>The rounded percentage of checked data lines (0 to 100).</summary>
        public int Progress
        {
            get
            {
                int ret = 0;

                try {
                    double curr = TestedCount + StartingPoint - 1;
                    double tot = DataPool.Size;
                    double ratio = curr / tot;
                    double percent = ratio * 100;
                    ret = (int)percent;
                }
                catch { }

                return Clamp(ret, 0, 100);
            }
        }

        private int cpm = 0;
        /// <summary>The checks per minute.</summary>
        public int CPM
        {
            get
            {
                if (TestedCount == 0)
                {
                    cpm = 0;
                    return cpm;
                }

                if (IsCPMLocked)
                {
                    return cpm;
                }

                try
                {
                    var temp = 0;
                    IsCPMLocked = true;
                    for (int i = FailedList.Count() - 1; i >= 0; i--) { if ((DateTime.Now - FailedList[i].Time).TotalSeconds > 60) break; temp++; }
                    for (int i = HitsList.Count() - 1; i >= 0; i--) { if ((DateTime.Now - HitsList[i].Time).TotalSeconds > 60) break; temp++; }
                    for (int i = CustomList.Count() - 1; i >= 0; i--) { if ((DateTime.Now - CustomList[i].Time).TotalSeconds > 60) break; temp++; }
                    for (int i = ToCheckList.Count() - 1; i >= 0; i--) { if ((DateTime.Now - ToCheckList[i].Time).TotalSeconds > 60) break; temp++; }
                    cpm = temp;
                    IsCPMLocked = false;
                }
                catch { }
                finally { IsCPMLocked = false; }

                return cpm;
            }
        }

        private double _balance = 0;
        /// <summary>The remaining balance in the captcha solver account.</summary>
        public double Balance { get { return _balance; } set { _balance = value; OnPropertyChanged("BalanceString"); } }

        /// <summary>The remaining balance in the captcha solver account preceeded by a $ sign.</summary>
        public string BalanceString { get { return $"${_balance}"; } }

        private ProxyMode proxyMode = ProxyMode.Default;
        /// <summary>The Proxy Mode.</summary>
        public ProxyMode ProxyMode { get { return proxyMode; } set { proxyMode = value; OnPropertyChanged(); } }

        /// <summary>Whether proxies can be used for the current session given the Proxy Mode and the Config.</summary>
        public bool UseProxies 
        { 
            get 
            {
                if (Config != null)
                {
                    return (Config.Settings.NeedsProxies && ProxyMode == ProxyMode.Default) || ProxyMode == ProxyMode.On;
                }
                else
                {
                    return true;
                }
            } 
        }

        /// <summary>The loaded Config to use for the check.</summary>
        public Config Config { get; private set; }

        /// <summary>The name of the loaded Config.</summary>
        public string ConfigName { get { return Config == null ? "None" : Config.Settings.Name; } }

        /// <summary>The loaded Wordlist to use for the check.</summary>
        public Wordlist Wordlist { get; private set; }

        /// <summary>The name of the loaded Wordlist.</summary>
        public string WordlistName { get { return Wordlist == null ? "None" : Wordlist.Name; } }

        /// <summary>The size of the loaded Wordlist.</summary>
        public int WordlistSize { get { return Wordlist == null ? 0 : Wordlist.Total; } }

        /// <summary>The pool from which bots can get a proxy upon request.</summary>
        public ProxyPool ProxyPool { get; set; } = new ProxyPool(new List<CProxy>());

        /// <summary>The total amount of proxies loaded.</summary>
        public int TotalProxiesCount { get { return ProxyPool.Proxies.Count; } }
        
        /// <summary>The amount of proxies loaded that are alive.</summary>
        public int AliveProxiesCount { get { return ProxyPool.Alive.Count; } }

        /// <summary>The amount of proxies loaded that are available.</summary>
        public int AvailableProxiesCount { get { return ProxyPool.Available.Count; } }

        /// <summary>The amount of proxies loaded that are banned.</summary>
        public int BannedProxiesCount { get { return ProxyPool.Banned.Count; } }

        /// <summary>The amount of proxies loaded that are bad.</summary>
        public int BadProxiesCount { get { return ProxyPool.Bad.Count; } }

        /// <summary>The pool from which the Master worker draws data to assign to bots for the checks.</summary>
        public DataPool DataPool { get; set; }

        /// <summary>The size of the DataPool.</summary>
        public int DataSize 
        { 
            get 
            { 
                if (DataPool != null)
                {
                    return DataPool.Size;
                }
                else
                {
                    return 0;
                }
            } 
        }
        #endregion

        #region Bot-Shared Fields
        /// <summary>The pairs of (variable name, value) set by the user.</summary>
        public List<KeyValuePair<string, string>> CustomInputs { get; set; } = new List<KeyValuePair<string, string>>();

        /// <summary>The Global Variables list that are set in all bots when they start.</summary>
        public VariableList GlobalVariables { get; set; } = new VariableList();

        /// <summary>The Global Cookies list that are set in all bots when they start.</summary>
        public CookieDictionary GlobalCookies { get; set; } = new CookieDictionary();

        /// <summary>If not null, bots will perform this action instead of the default behaviour.</summary>
        public Action<BotData> CustomAction { get; set; }
        #endregion

        #region Results
        /// <summary>The list of data lines checked with a FAIL outcome.</summary>
        public List<ValidData> FailedList { get; set; } = new List<ValidData>();

        /// <summary>The list of data lines checked with a SUCCESS outcome.</summary>
        public ObservableCollection<ValidData> HitsList { get; set; } = new ObservableCollection<ValidData>();

        /// <summary>The list of data lines checked with a CUSTOM outcome.</summary>
        public ObservableCollection<ValidData> CustomList { get; set; } = new ObservableCollection<ValidData>();

        /// <summary>The list of data lines checked with a NONE outcome.</summary>
        public ObservableCollection<ValidData> ToCheckList { get; set; } = new ObservableCollection<ValidData>();

        /// <summary>Auxiliary empty list.</summary>
        public ObservableCollection<ValidData> EmptyList { get; set; } = new ObservableCollection<ValidData>();

        /// <summary>The collection of data that was checked with a positive outcome.</summary>
        public IEnumerable<ValidData> Checked => (new IEnumerable<ValidData>[] { HitsList, CustomList, ToCheckList }).SelectMany(h => h).OrderBy(h => h.Time);

        /// <summary>Filter based on the Bot Status.</summary>
        public BotStatus ResultsFilter { get; set; } = BotStatus.SUCCESS;

        /// <summary>Amount of data lines checked with a FAIL outcome.</summary>
        public int FailCount { get { return FailedList.Count; } }

        /// <summary>Amount of data lines checked with a SUCCESS outcome.</summary>
        public int HitCount { get { return HitsList.Count; } }

        /// <summary>Amount of data lines checked with a CUSTOM outcome.</summary>
        public int CustomCount { get { return CustomList.Count; } }

        /// <summary>Amount of data lines checked with a NONE outcome.</summary>
        public int ToCheckCount { get { return ToCheckList.Count; } }

        private int retryCount = 0;
        /// <summary>Amount of data lines retried due to a BAN, RETRY or ERROR outcome.</summary>
        public int RetryCount { get { return retryCount; } set { retryCount = value; OnPropertyChanged(); } }

        /// <summary>Total amount of successfully tested data lines.</summary>
        public int TestedCount { get { return FailCount + HitCount + CustomCount + ToCheckCount; } }
        #endregion

        #region Stats
        /// <summary>
        /// Statistics of the checking process.
        /// </summary>
        public RunnerStats Stats => new RunnerStats(
                new RunnerStatsData(TestedCount, HitCount, CustomCount, FailCount, RetryCount, ToCheckCount),
                new RunnerStatsProxies(TotalProxiesCount, AliveProxiesCount, BannedProxiesCount, BadProxiesCount),
                CPM, (decimal)Balance);
        #endregion

        #region Locks
        /// <summary>Whether the workers are waiting for proxies to be reloaded.</summary>
        private bool IsReloadingProxies { get; set; } = false;
        
        /// <summary>Whether the CPM is already being calculated.</summary>
        private bool IsCPMLocked { get; set; } = false;
        
        /// <summary>Whether the warning about no proxy availability has already been issued.</summary>
        private bool NoProxyWarningSent { get; set; } = false;
        
        /// <summary>Whether the Custom Inputs have already been initialized.</summary>
        public bool CustomInputsInitialized { get; set; } = false;
        #endregion

        #region Timing
        private Stopwatch Timer { get; set; } = new Stopwatch();

        /// <summary>Days elapsed since the runner was started.</summary>
        public string TimerDays { get { return Timer.Elapsed.Days.ToString(); } }

        /// <summary>Hours elapsed since the runner was started.</summary>
        public string TimerHours { get { return Timer.Elapsed.Hours.ToString("D2"); } }

        /// <summary>Minutes elapsed since the runner was started.</summary>
        public string TimerMinutes { get { return Timer.Elapsed.Minutes.ToString("D2"); ; } }

        /// <summary>Seconds elapsed since the runner was started.</summary>
        public string TimerSeconds { get { return Timer.Elapsed.Seconds.ToString("D2"); ; } }

        /// <summary>Representation of the expected time left to check the remaining data lines.</summary>
        public string TimeLeft
        {
            get
            {
                if (Wordlist == null) return "Unknown time left";
                var dataLeft = Wordlist.Total - StartingPoint - TestedCount;
                if (CPM == 0) return "+inf";
                int amountLeft = (dataLeft / CPM) * 60; // in seconds
                var unitOfTime = "seconds";

                if (amountLeft > 60) { amountLeft /= 60; unitOfTime = "minutes"; } // More than 60s -> convert to minutes
                if (amountLeft > 60) { amountLeft /= 60; unitOfTime = "hours"; } // More than 60m -> convert to hours
                if (amountLeft > 24) { amountLeft /= 24; unitOfTime = "days"; } // More than 24h -> convert to days

                return $"{amountLeft} {unitOfTime} left";
            }
        }
        #endregion

        #region Setters
        /// <summary>
        /// Sets a Config in the Runner instance.
        /// </summary>
        /// <param name="config">The Config to be used for the check</param>
        /// <param name="setRecommended">Whether to automatically set the recommended amount of bots from the Config settings.</param>
        public void SetConfig(Config config, bool setRecommended)
        {
            Config = config;
            if (setRecommended) BotsAmount = Clamp(config.Settings.SuggestedBots, 1, 200);
            OnPropertyChanged("ConfigName");
            RaiseConfigChanged();
        }

        /// <summary>
        /// Sets a Wordlist in the Runner instance.
        /// </summary>
        /// <param name="wordlist">The Wordlist to be used for the check</param>
        public void SetWordlist(Wordlist wordlist)
        {
            Wordlist = wordlist;
            OnPropertyChanged("WordlistName");
            OnPropertyChanged("WordlistSize");
            RaiseWordlistChanged();
        }
        #endregion

        #region Worker Control Methods
        /// <summary>
        /// Starts the Master Worker.
        /// </summary>
        public void Start()
        {
            RaiseMessageArrived(LogLevel.Info, "Setting up the background worker", false);
            Master = new AbortableBackgroundWorker();
            Master.DoWork += new DoWorkEventHandler(Run);
            Master.RunWorkerCompleted += new RunWorkerCompletedEventHandler(RunCompleted);
            Master.WorkerSupportsCancellation = true;

            Timer.Reset();
            Timer.Start();

            if (!Master.IsBusy)
            {
                Master.Status = WorkerStatus.Running;
                OnPropertyChanged("Busy");
                OnPropertyChanged("ControlsEnabled");
                RaiseWorkerStatusChanged();
                Master.RunWorkerAsync();
                RaiseMessageArrived(LogLevel.Info, "Started the Master Worker", false);
            }
            else { RaiseMessageArrived(LogLevel.Error, "Cannot start the Background Worker (busy)", false); }
        }

        /// <summary>
        /// Stops the Master Worker.
        /// </summary>
        public void Stop()
        {
            if (Master.IsBusy) Master.CancelAsync();
            RaiseMessageArrived(LogLevel.Info, "Sent cancellation request to Master Worker", false);
            Master.Status = WorkerStatus.Stopping;
            OnPropertyChanged("Busy");
            OnPropertyChanged("ControlsEnabled");
            RaiseWorkerStatusChanged();
        }

        /// <summary>
        /// Forcefully aborts the Bots and stops the Master Worker.
        /// </summary>
        public void ForceStop()
        {
            AbortAllBots();
            Master.Abort();
            RaiseMessageArrived(LogLevel.Info, "Hard Aborted the Master Worker", false);
            Timer.Stop();
            Master.Status = WorkerStatus.Idle;
            OnPropertyChanged("Busy");
            OnPropertyChanged("ControlsEnabled");
            RaiseWorkerStatusChanged();
            StartingPoint += TestedCount;
        }
        #endregion

        #region Master Run Logic

        // Main job of the Master Worker
        private void Run(object sender, DoWorkEventArgs e)
        {
            // If a custom action was defined, set a default config to avoid Null Pointer Exceptions
            if (CustomAction != null) Config = new Config(new ConfigSettings(), "");

            if (Config == null) throw new Exception("No Config loaded!");
            if (Wordlist == null) throw new Exception("No Wordlist loaded!");

            if (!Wordlist.Temporary) DataPool = new DataPool(File.ReadLines(Wordlist.Path));
            RaiseMessageArrived(LogLevel.Info, $"Loaded {DataPool.Size} lines", false);
            RaiseMessageArrived(LogLevel.Info, $"Using Proxies: {UseProxies}", false);

            if (DataPool.Size == 0) throw new Exception("No data to process!");
            if (StartingPoint > DataPool.Size) throw new Exception("Illegal Starting Point!");
            if (CustomAction == null && Config.BlocksAmount == 0) throw new Exception("The Config has zero blocks!");

            // Reset the stats
            NoProxyWarningSent = false;
            CustomInputsInitialized = false;
            RetryCount = 0;
            FailedList.Clear();
            GlobalVariables = new VariableList();
            GlobalCookies = new CookieDictionary();

            // We need to dispatch this to the main thread because these change the observable collections, hence changing the UI
            RaiseDispatchAction(new Action(() =>
            {
                HitsList.Clear();
                CustomList.Clear();
                ToCheckList.Clear();
            }));

            // Load the Proxies
            if (UseProxies)
            {
                LoadProxies();

                if (TotalProxiesCount == 0) throw new Exception("Zero proxies available!");
            }

            // Ask for the inputs
            if (Config.Settings.CustomInputs.Count > 0)
            {
                RaiseAskCustomInputs();
                while (!CustomInputsInitialized) Thread.Sleep(100);
            }

            // Empty the bots list (dispatch it to the main thread because it's observable)
            RaiseDispatchAction(new Action(() => Bots.Clear()));

            // Create the given amount of bots and assign them their DoWork function
            for (int i = 1; i <= BotsAmount; i++)
            {
                RaiseMessageArrived(LogLevel.Info, $"Creating bot {i}", false);
                var bot = new RunnerBotViewModel(i);
                if (Settings.General.BotsDisplayMode == BotsDisplayMode.None)
                    bot.Status = "Bots Display is Disabled in Settings";
                bot.Worker.DoWork += new DoWorkEventHandler(RunBot);
                RaiseDispatchAction(new Action(() => Bots.Add(bot)));
            }
            
            // Checking Process
            foreach (var data in DataPool.List.Skip(StartingPoint - 1))
            {
                // Check if there is a cancellation request
                if (Master.CancellationPending)
                {
                    AbortAllBots();
                    return;
                }

                // Create an instance of CData basing on the current data line
                CData c = new CData(data, Env.GetWordlistType(Wordlist.Type));

                // Check if it's valid
                if (!c.IsValid || !c.RespectsRules(Config.Settings.DataRules.ToList()))
                {
                    FailedList.Add(new ValidData(data, "", ProxyType.Http, BotStatus.FAIL, "", "", "", null));
                    continue;
                }

                // Update the observable properties
                UpdateTimer();
                UpdateStats();
                UpdateCPM();

                // If the amount of Bots was changed
                if (BotsAmount != Bots.Count)
                {
                    RaiseMessageArrived(LogLevel.Info, $"Bots Number was changed from {Bots.Count} to {BotsAmount}", false);

                    // If it was increased
                    if (BotsAmount > Bots.Count)
                    {
                        // Create the missing bots
                        for (int b = Bots.Count + 1; b <= BotsAmount; b++)
                        {
                            RaiseMessageArrived(LogLevel.Info, $"Creating bot {b}", false);
                            try
                            {
                                var bot = new RunnerBotViewModel(b);
                                if (Settings.General.BotsDisplayMode == BotsDisplayMode.None)
                                    bot.Status = "Bots Display is Disabled in Settings";
                                bot.Worker.DoWork += new DoWorkEventHandler(RunBot);
                                RaiseDispatchAction(new Action(() => Bots.Add(bot)));
                            }
                            catch (Exception ex) { RaiseMessageArrived(LogLevel.Error, $"Error while creating bot {b}: " + ex.Message, false); }
                        }
                    }

                    // If it was decreased
                    else
                    {
                        // Terminate the unnecessary bots
                        for (int b = Bots.Count - 1; b >= BotsAmount; b--)
                        {
                            RaiseMessageArrived(LogLevel.Info, $"Removing bot {b}", false);
                            try
                            {
                                var bot = Bots[b];
                                if (bot.IsDriverOpen) bot.Driver.Quit();
                                bot.Worker.CancelAsync();
                                bot.Worker.Abort();
                                RaiseDispatchAction(new Action(() => Bots.RemoveAt(b)));
                            }
                            catch (Exception ex) { RaiseMessageArrived(LogLevel.Error, $"Error while creating bot {b}: " + ex.Message, false); }
                        }
                    }
                }

                RaiseMessageArrived(LogLevel.Info, $"Trying to assign data " + data, false);
                bool assigned = false;

                // Assignment of a data line to a bot
                while (!assigned)
                {
                    // If we reached the max number of hits, abort
                    if (Settings.General.MaxHits != 0 && HitCount >= Settings.General.MaxHits)
                    {
                        AbortAllBots();
                        return;
                    }

                    // Check if there is a cancellation request
                    if (Master.CancellationPending)
                    {
                        AbortAllBots();
                        return;
                    }

                    // Periodic actions
                    if (Timer.IsRunning && (int)Timer.Elapsed.TotalSeconds != 0)
                    {
                        // Write progress to DB every 2 minutes
                        // Useful if we cannot save it upon work completion (e.g. for a crash or power outage)
                        if ((int)Timer.Elapsed.TotalSeconds % 120 == 0)
                        {
                            RaiseSaveProgress();
                        }

                        // Reload proxies if a reload interval was set
                        if (Settings.Proxies.ReloadInterval > 0 && (int)Timer.Elapsed.TotalSeconds % (Settings.Proxies.ReloadInterval * 60) == 0)
                        {
                            LoadProxies();
                        }
                    }

                    // If we are above the CPM limit, go to the wait (use cpm NOT CPM so it doesn't calculate it uselessly when it checks the IF conditions)
                    if (Config.Settings.MaxCPM > 0 && cpm >= Config.Settings.MaxCPM)
                    {
                        // Update the CPM and go directly to the wait (which will update the other stats too)
                        UpdateCPM();
                    }
                    else
                    {
                        // Search for the first available bot, assign the data to it and start its job
                        foreach (var bot in Bots)
                        {
                            if (!bot.Worker.IsBusy)
                            {
                                RaiseMessageArrived(LogLevel.Info, "Assigned data " + data + " to bot " + bot.Id, false);
                                bot.Worker.RunWorkerAsync(data);
                                assigned = true;
                                break;
                            }
                        }
                    }

                    //If all bots are busy, sleep a while before checking again
                    if (!assigned)
                    {
                        UpdateTimer();
                        UpdateStats();
                        // Do not update CPM because it would be very CPU demanding

                        Thread.Sleep(200);
                    }
                }
            }

            // Wait until all threads have finished their last job and keep updating the observable properties
            while (Bots.Select(b => b.Worker).Any(w => w.IsBusy) && !Master.CancellationPending)
            {
                RaiseMessageArrived(LogLevel.Info, "All data assigned, waiting for completion", false);
                UpdateStats();
                UpdateCPM();
                Thread.Sleep(200);
            }
        }

        // Executed when the Master Worker has finished its job
        private void RunCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (Settings.General.SendToCheckOnAbort)
            {
                foreach (var bot in Bots.Where(b => b.Worker.IsBusy))
                {
                    ValidData validData = new ValidData(bot.Data, bot.Proxy, ProxyType.Http, BotStatus.NONE, "NONE", "", "", new List<LogEntry>());
                    ToCheckList.Add(validData);
                    UpdateStats();
                    var hit = new Hit(bot.Data, new VariableList(), bot.Proxy, "NONE", ConfigName, WordlistName);
                    RaiseFoundHit(hit);
                }
            }

            if (e.Error != null) RaiseMessageArrived(LogLevel.Error, "The Master Worker has encountered an error: " + e.Error.Message, true);
            Master.Status = WorkerStatus.Idle;
            OnPropertyChanged("Busy");
            OnPropertyChanged("ControlsEnabled");
            RaiseWorkerStatusChanged();
            Timer.Stop();
            AbortAllBots();
            StartingPoint += TestedCount;
        }
        #endregion

        #region Bot Run Logic

        // Bot job logic
        private void RunBot(object sender, DoWorkEventArgs e)
        {
            // Get a reference to the Worker that is executing the job and the corresponding Bot
            var senderABW = (AbortableBackgroundWorker)sender;
            var bot = Bots.First(b => b.Id == (senderABW.Id));
            bot.Status = "INITIALIZING...";

            // The data line that needs to be processed
            var data = (string)e.Argument;
            bot.Data = data;
            CData currentData = new CData(data, Env.GetWordlistType(Wordlist.Type));

            RaiseMessageArrived(LogLevel.Info, $"[{bot.Id}] started with data " + bot.Data, false);

            try
            {
                GETPROXY:

                // Check if the job was cancelled or if the Master Worker is not running
                if (senderABW.CancellationPending || ShouldStop())
                {
                    RaiseMessageArrived(LogLevel.Info, $"[{bot.Id}] Cancellation pending, aborting", false);
                    return;
                }

                CProxy currentProxy = null;
                
                // If the config requires proxies or we enforced proxy use
                if (UseProxies)
                {
                    // Try to get a proxy from the pool
                    currentProxy = ProxyPool.GetProxy(Settings.Proxies.ConcurrentUse, Config.Settings.MaxProxyUses, Settings.Proxies.NeverBan);

                    // If there are no more proxies available
                    if (currentProxy == null)
                    {
                        // If we can reload them from the proxy source
                        if (Settings.Proxies.Reload)
                        {
                            // If no one else is already reloading the proxies, reload them
                            if (!IsReloadingProxies)
                            {
                                IsReloadingProxies = true;
                                LoadProxies();
                                IsReloadingProxies = false;
                            }

                            // If there is already someone else reloading the proxies, try again until they finished
                            else
                            {
                                Thread.Sleep(100);
                                goto GETPROXY;
                            }
                        }

                        // If we cannot reload
                        else
                        {
                            // If the Master Worker is busy
                            if (!ShouldStop())
                            {
                                // If no one else issued the no more proxies warning message, issue it
                                if (!NoProxyWarningSent)
                                {
                                    RaiseMessageArrived(LogLevel.Error, "No more proxies and no Unban All option selected OR the config has a max proxy use! Aborting", true, 2);
                                    NoProxyWarningSent = true;
                                }

                                Master.CancelAsync();
                                return;
                            }
                        }
                    }

                    // Assign the proxy to the bot's displayed fields
                    bot.Proxy = currentProxy.Proxy;
                }
                var proxyUsedText = currentProxy == null ? "NONE" : $"{currentProxy.Proxy} ({currentProxy.Type})";

                // Initialize the Bot Data
                BotData botData = null;
                lock (randomLocker)
                {
                    botData = new BotData(Settings, Config.Settings, currentData, currentProxy, UseProxies, random, bot.Id, false);
                }
                botData.Driver = bot.Driver;
                botData.BrowserOpen = bot.IsDriverOpen;
                List<LogEntry> BotLog = new List<LogEntry>();

                // Set the variables from the Custom Inputs
                foreach (var pair in CustomInputs)
                    botData.Variables.Set(new CVar(pair.Key, pair.Value));

                // Set the global variables (pass by reference so every bot acts on the same list)
                botData.GlobalVariables = GlobalVariables;

                // Set the global cookies
                try
                {
                    foreach (var cookie in GlobalCookies)
                        botData.Cookies.Add(cookie.Key, cookie.Value);
                }
                catch { }

                // Set the cloudflare cookies (to be used in normal requests) if we already have clearance and we don't have to get it each time
                if (botData.UseProxies && botData.Proxy != null && botData.Proxy.Clearance != string.Empty && !Settings.Proxies.AlwaysGetClearance)
                {
                    botData.Cookies["cf_clearance"] = botData.Proxy.Clearance;
                    botData.Cookies["__cfduid"] = botData.Proxy.Cfduid;
                }

                // Print the start message
                BotLog.Add(new LogEntry($"===== LOG FOR BOT #{bot.Id} WITH DATA {botData.Data.Data} AND PROXY {proxyUsedText} ====={Environment.NewLine}", Colors.White));

                if (CustomAction != null)
                {
                    RaiseMessageArrived(LogLevel.Info, $"[{bot.Id}] Executing custom action", false);
                    try
                    {
                        CustomAction.Invoke(botData);
                    }
                    catch (Exception ex) { RaiseMessageArrived(LogLevel.Info, $"[{bot.Id}] CUSTOM ACTION EXCEPTION: {ex.ToString()}", false); throw; }
                    goto FINISH;
                }

                /* =================
                 * SCRIPT PROCESSING *
                   ================= */

                // Initialize the LoliScript
                LoliScript loli = new LoliScript(Config.Script);
                loli.Reset();

                if (!Settings.General.EnableBotLog) BotLog.Add(new LogEntry("The Bot Logging is disabled in General Settings", Colors.Tomato));

                // Open browser if Always Open
                if (Config.Settings.AlwaysOpen)
                    SBlockBrowserAction.OpenBrowser(botData);

                do
                {
                    // Check if the job was cancelled or if the Master Worker is not running
                    if (senderABW.CancellationPending || ShouldStop())
                    {
                        RaiseMessageArrived(LogLevel.Info, $"[{bot.Id}] Cancellation pending, aborting", false);
                        return;
                    }

                    // Output the label of the current block being processed
                    if (Settings.General.BotsDisplayMode == BotsDisplayMode.Everything)
                        bot.Status = "<<< PROCESSING BLOCK: " + loli.NextBlock + " >>>";

                    // Try to take a step in the LoliScript and output any errors
                    try
                    {
                        loli.TakeStep(botData);
                    }
                    catch (BlockProcessingException ex)
                    {
                        if (Settings.General.BotsDisplayMode == BotsDisplayMode.Everything)
                            bot.Status = $"<<< ERROR IN BLOCK: {loli.NextBlock} >>>";
                        RaiseMessageArrived(LogLevel.Error, $"[{bot.Id}][{bot.Data}][{proxyUsedText}] ERROR in block {loli.NextBlock} | Exception: {ex.Message}", false);
                        Thread.Sleep(1000);
                    }
                    catch (Exception ex)
                    {
                        if (Settings.General.BotsDisplayMode == BotsDisplayMode.Everything)
                            bot.Status = "<<< SCRIPT ERROR >>>";
                        RaiseMessageArrived(LogLevel.Error, $"[{bot.Id}][{bot.Data}][{proxyUsedText}] ERROR in the script | Exception: {ex.Message}", false);
                        Thread.Sleep(1000);
                    }

                    // Save the contents of the LogBuffer (that gets cleaned with every TakeStep() call) to the BotLog
                    if (botData.LogBuffer.Count > 0)
                    {
                        BotLog.AddRange(botData.LogBuffer);
                        BotLog.Add(new LogEntry("", Colors.White));
                    }
                }
                while (loli.CanProceed); // Do this while the LoliScript has stuff to process

                FINISH:

                // Print the end message
                BotLog.Add(new LogEntry($"===== BOT TERMINATED WITH RESULT: {botData.StatusString} =====", Colors.White));
                if (Settings.General.BotsDisplayMode != BotsDisplayMode.None)
                    bot.Status = $"<<< FINISHED WITH RESULT: {botData.StatusString} >>>";

                RaiseMessageArrived(LogLevel.Info, $"[{bot.Id}][{bot.Data}][{proxyUsedText}] Ended with result {botData.StatusString}", false);

                // Quit Browser if Always Quit
                if (Config.Settings.AlwaysQuit || (Config.Settings.QuitOnBanRetry && (botData.Status == BotStatus.BAN || botData.Status == BotStatus.RETRY)))
                    try { botData.Driver.Quit(); botData.BrowserOpen = false; } catch { }

                // Save Browser Status
                bot.Driver = botData.Driver;
                bot.IsDriverOpen = botData.BrowserOpen;

                // Put the proxy status back to available and increment the uses
                if (UseProxies)
                {
                    currentProxy.Status = Status.AVAILABLE;
                    currentProxy.Uses++;
                    currentProxy.Hooked--;
                }

                // Update the captcha service balance
                Balance = botData.Balance;

                // Push the Global Cookies to the shared Dictionary
                try
                {
                    GlobalCookies = new CookieDictionary();
                    foreach (var cookie in botData.GlobalCookies)
                        GlobalCookies.Add(cookie.Key, cookie.Value);
                }
                catch { }

                // Analyze the Result of the Check
                ValidData validData = null;
                string hitType = botData.Status.ToString();
                
                // Build the Captured Data
                VariableList capturedData = new VariableList(botData.Variables.All.Where(v => v.IsCapture && !v.Hidden).ToList());

                // Detect the result of the check and act accordingly
                switch (botData.Status)
                {
                    case BotStatus.SUCCESS:
                        validData = new ValidData(botData.Data.Data, botData.Proxy == null ? "" : botData.Proxy.Proxy, botData.Proxy == null ? ProxyType.Http : botData.Proxy.Type, botData.Status, "HIT", capturedData.ToCaptureString(), Settings.General.SaveLastSource ? botData.ResponseSource : "", BotLog);
                        RaiseDispatchAction(new Action(() => HitsList.Add(validData)));
                        if (UseProxies && !Settings.Proxies.NeverBan && Config.Settings.BanProxyAfterGoodStatus)
                        {
                            currentProxy.Status = Status.BANNED;
                            currentProxy.LastUsed = DateTime.Now;
                            RetryCount++;
                        }

                        break;

                    case BotStatus.FAIL:
                        FailedList.Add(new ValidData("", "", ProxyType.Http, botData.Status, "", "", "", null)); // Only needed for CPM calculation so we can set blank data to avoid filling RAM
                        break;

                    case BotStatus.CUSTOM:
                        hitType = botData.CustomStatus;
                        validData = new ValidData(botData.Data.Data, botData.Proxy == null ? "" : botData.Proxy.Proxy, botData.Proxy == null ? ProxyType.Http : botData.Proxy.Type, botData.Status, hitType, capturedData.ToCaptureString(), Settings.General.SaveLastSource ? botData.ResponseSource : "", BotLog);
                        RaiseDispatchAction(new Action(() => CustomList.Add(validData)));

                        if (UseProxies && !Settings.Proxies.NeverBan && Config.Settings.BanProxyAfterGoodStatus)
                        {
                            currentProxy.Status = Status.BANNED;
                            currentProxy.LastUsed = DateTime.Now;
                            RetryCount++;
                        }
                        break;

                    case BotStatus.BAN:
                        // If the NeverBan option is true or we don't use a proxy, the BAN gets treated as a RETRY
                        if (UseProxies && !Settings.Proxies.NeverBan)
                        {
                            currentProxy.Status = Status.BANNED;
                            currentProxy.LastUsed = DateTime.Now;
                            RetryCount++;
                        }

                        if (ShouldTriggerEvasion(currentData.Retries))
                        {
                            currentData.Retries++;
                            goto GETPROXY;
                        }
                        else
                        {
                            RaiseMessageArrived(LogLevel.Warning, $"[{bot.Id}][{bot.Data}] Maximum retries exceeded");
                            botData.Status = BotStatus.NONE;
                            hitType = botData.Status.ToString();
                            goto TOCHECK;
                        }

                    case BotStatus.ERROR: // We assume it's a proxy error and that the Config is working correctly, so we mark the proxy as bad
                        RetryCount++;
                        if (UseProxies) currentProxy.Status = Status.BAD;
                        goto GETPROXY;

                    case BotStatus.RETRY:
                        RetryCount++;
                        goto GETPROXY;

                    case BotStatus.NONE:
                        TOCHECK:
                        validData = new ValidData(botData.Data.Data, botData.Proxy == null ? "" : botData.Proxy.Proxy, botData.Proxy == null ? ProxyType.Http : botData.Proxy.Type, botData.Status, "TOCHK", capturedData.ToCaptureString(), Settings.General.SaveLastSource ? botData.ResponseSource : "", BotLog);
                        RaiseDispatchAction(new Action(() => ToCheckList.Add(validData)));

                        if (UseProxies && !Settings.Proxies.NeverBan && Config.Settings.BanProxyAfterGoodStatus)
                        {
                            currentProxy.Status = Status.BANNED;
                            currentProxy.LastUsed = DateTime.Now;
                            RetryCount++;
                        }
                        break;
                }

                // Add it to the List and to the Database as well
                if (validData != null)
                {
                    var hit = new Hit(botData.Data.Data, capturedData, currentProxy == null ? "" : currentProxy.Proxy, hitType, ConfigName, WordlistName);
                    RaiseFoundHit(hit);
                }

                // Call the webhook
                if (Settings.General.WebhookEnabled && (botData.Status == BotStatus.SUCCESS || botData.Status == BotStatus.CUSTOM))
                {
                    HttpRequest request = new HttpRequest();
                    try
                    {
                        var toSend = new WebhookFormat(data, hitType, capturedData.ToCaptureString(), DateTime.Now, Config.Settings.Name, Config.Settings.Author, Settings.General.WebhookUser);
                        var json = JsonConvert.SerializeObject(toSend);
                        request.PostAsync(Settings.General.WebhookURL, json, "application/json");
                    }
                    catch
                    {
                        RaiseMessageArrived(LogLevel.Error, $"Could not register the hit to webhook {Settings.General.WebhookURL}");
                    }
                }

                // Wait time
                if (Settings.General.WaitTime > 0)
                    Thread.Sleep(Settings.General.WaitTime);
            }
            catch (Exception ex) { RaiseMessageArrived(LogLevel.Error, $"[{bot.Id}] Check exception on data " + data + $" - {ex.Message}", false); }
        }

        /// <summary>
        /// Checks if the Master Worker is running and has not been cancelled.
        /// </summary>
        /// <returns>True if the Master Worker is stopping or has already stopped</returns>
        private bool ShouldStop()
        {
            return Master.CancellationPending || Master.Status != WorkerStatus.Running;
        }
        #endregion

        #region Update Methods

        /// <summary>
        /// Update the observable properties for the Timer.
        /// </summary>
        public void UpdateTimer()
        {
            OnPropertyChanged("TimerDays");
            OnPropertyChanged("TimerHours");
            OnPropertyChanged("TimerMinutes");
            OnPropertyChanged("TimerSeconds");
            OnPropertyChanged("TimeLeft");
        }

        /// <summary>
        /// Update the observable properties for the statistics.
        /// </summary>
        public void UpdateStats()
        {
            OnPropertyChanged("HitsList");
            OnPropertyChanged("FreeList");
            OnPropertyChanged("ToCheckList");
            OnPropertyChanged("TestedCount");
            OnPropertyChanged("ProgressCount");
            OnPropertyChanged("FailCount");
            OnPropertyChanged("HitCount");
            OnPropertyChanged("CustomCount");
            OnPropertyChanged("ToCheckCount");
            OnPropertyChanged("Balance");
            OnPropertyChanged("Progress");

            OnPropertyChanged("TotalProxiesCount");
            OnPropertyChanged("AvailableProxiesCount");
            OnPropertyChanged("AliveProxiesCount");
            OnPropertyChanged("BannedProxiesCount");
            OnPropertyChanged("BadProxiesCount");
        }

        /// <summary>
        /// Update the observable CPM property.
        /// </summary>
        public void UpdateCPM()
        {
            OnPropertyChanged("CPM");
        }
        #endregion

        #region Events
        /// <summary>Fired when a new message needs to be logged.</summary>
        public event Action<IRunnerMessaging, LogLevel, string, bool, int> MessageArrived;

        /// <summary>Fired when the Master Worker status changed.</summary>
        public event Action<IRunnerMessaging> WorkerStatusChanged;

        /// <summary>Fired when a Hit was found.</summary>
        public event Action<IRunnerMessaging, Hit> FoundHit;

        /// <summary>Fired when proxies need to be reloaded.</summary>
        public event Action<IRunnerMessaging> ReloadProxies;

        /// <summary>/// Fired when an Action could change the UI and needs to be dispatched to another thread (usually it's handled by the UI thread).</summary>
        public event Action<IRunnerMessaging, Action> DispatchAction;

        /// <summary>Fired when the progress record needs to be saved to the Database.</summary>
        public event Action<IRunnerMessaging> SaveProgress;

        /// <summary>Fired when custom inputs from the user are required.</summary>
        public event Action<IRunnerMessaging> AskCustomInputs;

        /// <summary>Fired when the currently selected Config changed.</summary>
        public event Action<IRunnerMessaging> ConfigChanged;
        
        /// <summary>Fired when the currently selected Wordlist changed.</summary>
        public event Action<IRunnerMessaging> WordlistChanged;

        private void RaiseMessageArrived(LogLevel level, string message, bool prompt = false, int timeout = 0)
        {
            MessageArrived?.Invoke(this, level, message, prompt, timeout);
        }

        private void RaiseWorkerStatusChanged()
        {
            OnPropertyChanged("WorkerStatus");
            WorkerStatusChanged?.Invoke(this);
        }

        private void RaiseFoundHit(Hit hit)
        {
            FoundHit?.Invoke(this, hit);
        }

        private void RaiseReloadingProxies()
        {
            ReloadProxies?.Invoke(this);
        }

        private void RaiseDispatchAction(Action action)
        {
            DispatchAction?.Invoke(this, action);
        }

        private void RaiseSaveProgress()
        {
            SaveProgress?.Invoke(this);
        }

        private void RaiseAskCustomInputs()
        {
            AskCustomInputs?.Invoke(this);
        }

        private void RaiseConfigChanged()
        {
            ConfigChanged?.Invoke(this);
        }

        private void RaiseWordlistChanged()
        {
            WordlistChanged?.Invoke(this);
        }
        #endregion

        #region Proxy Management Methods

        /// <summary>
        /// Loads the proxies from the specified source.
        /// </summary>
        private void LoadProxies()
        {
            RaiseMessageArrived(LogLevel.Info, $"Loading proxies from {Settings.Proxies.ReloadSource}", false);

            switch (Settings.Proxies.ReloadSource)
            {
                case ProxyReloadSource.Manager:
                    // Raise the event and wait a bit so the proxies are loaded from the manager.
                    RaiseReloadingProxies();
                    Thread.Sleep(100);
                    break;

                case ProxyReloadSource.Remote:
                    List<CProxy> proxies = new List<CProxy>();
                    Parallel.ForEach(Settings.Proxies.RemoteProxySources.Where(s => s.Active), s =>
                    {
                        try
                        {
                            proxies.AddRange(GetProxiesFromRemoteSource(s.Url, s.Type, s.Pattern, s.Output));
                        }
                        catch (Exception ex) { RaiseMessageArrived(LogLevel.Error, $"Could not contact the reload API {s.Url} for {s.Type} proxies - {ex.Message}", true, 5); }
                    });
                    ProxyPool = new ProxyPool(proxies, Settings.Proxies.ShuffleOnStart);
                    // ProxyPool.RemoveDuplicates();
                    break;

                case ProxyReloadSource.File:
                    try
                    {
                        ProxyPool = new ProxyPool(
                            GetProxiesFromFile(Settings.Proxies.ReloadPath,
                                                Settings.Proxies.ReloadType), Settings.Proxies.ShuffleOnStart);
                        // ProxyPool.RemoveDuplicates();
                    }
                    catch (Exception ex) { RaiseMessageArrived(LogLevel.Error, $"Could not read the proxies from file {Settings.Proxies.ReloadPath} - {ex.Message}", true); }
                    break;
            }

            RaiseMessageArrived(LogLevel.Info, $"Loaded {TotalProxiesCount} proxies", false);
        }

        /// <summary>
        /// Loads a list of proxies from a remote source.
        /// </summary>
        /// <param name="url">The URL of the remote source</param>
        /// <param name="type">The type of the proxies</param>
        /// <param name="pattern">The Regex pattern to be used for parsing the proxies</param>
        /// <param name="output">The output format of the groups matched by the regex</param>
        /// <returns>The list of CProxy objects loaded from the API</returns>
        public static List<CProxy> GetProxiesFromRemoteSource(string url, ProxyType type, string pattern, string output)
        {
            var proxies = new List<CProxy>();

            HttpRequest req = new HttpRequest();
            req.ConnectTimeout = 5000;
            req.ReadWriteTimeout = 5000;
            req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/65.0.3325.181 Safari/537.36 OPR/52.0.2871.64";
            var resp = req.Get(url).ToString();

            try
            {
                var matches = Regex.Matches(resp, pattern);
                foreach (Match match in matches)
                {
                    var result = output;
                    for (var i = 0; i < match.Groups.Count; i++) result = result.Replace("[" + i + "]", match.Groups[i].Value);
                    proxies.Add(new CProxy(result, type));
                }
            }
            catch { }

            return proxies;
        }

        /// <summary>
        /// Loads a list of proxies from a remote source asynchronously.
        /// </summary>
        /// <param name="url">The URL of the remote source</param>
        /// <param name="type">The type of the proxies</param>
        /// <param name="pattern">The Regex pattern to be used for parsing the proxies</param>
        /// <param name="output">The output format of the groups matched by the regex</param>
        /// <returns>The list of CProxy objects loaded from the API</returns>
        public async static Task<RemoteProxySourceResult> GetProxiesFromRemoteSourceAsync(string url, ProxyType type, string pattern, string output)
        {
            var proxies = new List<CProxy>();

            try
            {
                HttpRequest req = new HttpRequest();
                req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/65.0.3325.181 Safari/537.36 OPR/52.0.2871.64";
                var resp = (await req.GetAsync(url)).ToString();

                var matches = Regex.Matches(resp, pattern);
                foreach (Match match in matches)
                {
                    var result = output;
                    for (var i = 0; i < match.Groups.Count; i++) result = result.Replace("[" + i + "]", match.Groups[i].Value);
                    proxies.Add(new CProxy(result, type));
                }
            }
            catch (Exception ex)
            {
                return new RemoteProxySourceResult()
                {
                    Successful = false,
                    Error = ex.Message,
                    Url = url,
                    Proxies = new List<CProxy>()
                };
            }

            return new RemoteProxySourceResult()
            {
                Successful = true,
                Error = "",
                Url = url,
                Proxies = proxies
            };
        }

        /// <summary>
        /// Loads a list of proxies from a file.
        /// </summary>
        /// <param name="fileName">The file containing the proxies, one per line</param>
        /// <param name="type">The type of the proxies</param>
        /// <returns>The list of CProxy objects loaded from the file</returns>
        public static List<CProxy> GetProxiesFromFile(string fileName, ProxyType type)
        {
            var lines = File.ReadAllLines(fileName);
            var proxies = new List<CProxy>();

            proxies.AddRange(lines.Select(l => new CProxy(l, type)));

            return proxies;
        }

        private bool ShouldTriggerEvasion(int retries)
        {
            var evasionValue = Config.Settings.BanLoopEvasionOverride == -1 
                ? Settings.Proxies.BanLoopEvasion 
                : Config.Settings.BanLoopEvasionOverride;
            
            return retries < evasionValue || evasionValue == 0;
        }
        #endregion

        #region Bot Management Methods

        /// <summary>
        /// Cancels the async job on all bots and quits all the drivers.
        /// </summary>
        private void AbortAllBots()
        {
            foreach (var bot in Bots)
            {
                try
                {
                    bot.Worker.CancelAsync();
                }
                catch (Exception ex) { RaiseMessageArrived(LogLevel.Error, $"Error while deleting bot {bot.Id}: " + ex.Message, false); }
            }
            QuitAllDrivers();
        }

        /// <summary>
        /// Quits all the open selenium drivers.
        /// </summary>
        private void QuitAllDrivers()
        {
            foreach (var bot in Bots)
            {
                try
                {
                    if (bot.IsDriverOpen) bot.Driver.Quit();
                }
                catch (Exception ex) { RaiseMessageArrived(LogLevel.Error, $"Error while quitting driver {bot.Id}: " + ex.Message, false); }
            }
        }
        #endregion

        #region Utility Methods

        /// <summary>
        /// Clamps an integer to an interval of values.
        /// </summary>
        /// <param name="a">The integer to clamp</param>
        /// <param name="min">The minimum value of the interval</param>
        /// <param name="max">The maximum value of the interval</param>
        /// <returns>The clamped integer</returns>
        public int Clamp(int a, int min, int max)
        {
            if (a < min) return min;
            else if (a > max) return max;
            else return a;
        }
        #endregion
    }
}
