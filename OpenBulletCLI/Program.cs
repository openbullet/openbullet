using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using CommandLine;
using CommandLine.Text;
using Extreme.Net;
using RuriLib;
using RuriLib.Models;
using RuriLib.Runner;
using RuriLib.ViewModels;
using Console = Colorful.Console;

namespace OpenBulletCLI
{
    /*
     * THIS IS A POC (Proof Of Concept) IMPLEMENTATION OF RuriLib IN CLI (Command Line Interface).
     * The functionalities supported here don't even come close to the ones of the WPF GUI implementation.
     * Feel free to contribute to the versatility of this project by adding the missing functionalities.
     * 
     * */


    class Program
    {
        private static string envFile = @"Settings/Environment.ini";
        private static string settFile = @"Settings/RLSettings.json";
        private static string outFile = "";

        public static EnvironmentSettings Env { get; set; }
        public static RLSettingsViewModel RLSettings { get; set; }
        private static Random random = new Random();
        public static RunnerViewModel Runner { get; set; }
        public static bool Verbose { get; set; } = false;
        public static string ProxyFile { get; set; }
        public static ProxyType ProxyType { get; set; }

        class Options
        {
            [Option('c', "config", Required = true, HelpText = "Configuration file to be processed.")]
            public string ConfigFile { get; set; }

            [Option('w', "wordlist", Required = true, HelpText = "Wordlist file to be processed.")]
            public string WordlistFile { get; set; }

            [Option('o', "output", Default = "", HelpText = "Output file for the hits.")]
            public string OutputFile { get; set; }

            [Option("wltype", Required = true, HelpText = "Type of the wordlist loaded (see Environment.ini for all allowed types).")]
            public string WordlistType { get; set; }

            [Option("useproxies", HelpText = "Enable / Disable the usage of proxies (uses config default if not set).")]
            public bool? UseProxies { get; set; }

            [Option('p', "proxies", Default = null, HelpText = "Proxy file to be processed.")]
            public string ProxyFile { get; set; }

            [Option("ptype", Default = ProxyType.Http, HelpText = "Type of proxies loaded (Http, Socks4, Socks4a, Socks5).")]
            public ProxyType ProxyType { get; set; }

            [Option('v', "verbose", Default = false, HelpText = "Prints all bots behaviour.")]
            public bool Verbose { get; set; }

            [Option('s', "skip", Default = 1, HelpText = "Number of lines to skip in the Wordlist.")]
            public int Skip { get; set; }

            [Option('b', "bots", Default = 0, HelpText = "Number of concurrent bots working. If not specified, the config default will be used.")]
            public int BotsNumber { get; set; }

            [Usage(ApplicationAlias = "OpenBulletCLI.exe")]
            public static IEnumerable<Example> Examples
            {
                get
                {
                    return new List<Example>() {
                        new Example("Simple POC CLI Implementation of RuriLib that executes a Runner.", 
                            new Options {
                                ConfigFile = "config.loli",
                                WordlistFile = "rockyou.txt",
                                WordlistType = "Default",
                                ProxyFile = "proxies.txt",
                                OutputFile = "hits.txt",
                                ProxyType = ProxyType.Http,
                                UseProxies = true,
                                Verbose = false,
                                Skip = 1,
                                BotsNumber = 1
                            }
                        )
                    };
                }
            }
        }

        static void Main(string[] args)
        {
            // Read Environment file
            Env = IOManager.ParseEnvironmentSettings(envFile);

            // Read Settings file
            if (!File.Exists(settFile)) IOManager.SaveSettings(settFile, new RLSettingsViewModel());
            RLSettings = IOManager.LoadSettings<RLSettingsViewModel>(settFile);

            // Initialize the Runner (and hook event handlers)
            Runner = new RunnerViewModel(Env, RLSettings, random);
            Runner.AskCustomInputs += AskCustomInputs;
            Runner.DispatchAction += DispatchAction;
            Runner.FoundHit += FoundHit;
            Runner.MessageArrived += MessageArrived;
            Runner.ReloadProxies += ReloadProxies;
            Runner.SaveProgress += SaveProgress;
            Runner.WorkerStatusChanged += WorkerStatusChanged;

            // Parse the Options
            CommandLine.Parser.Default.ParseArguments<Options>(args)
              .WithParsed<Options>(opts => Run(opts))
              .WithNotParsed<Options>((errs) => HandleParseError(errs));
        }

        private static void WorkerStatusChanged(IRunnerMessaging obj)
        {
            // Nothing to do here since the Title is updated every 100 ms anyways
        }

        private static void SaveProgress(IRunnerMessaging obj)
        {
            // TODO: Implement progress saving (maybe to a file)
        }

        private static void ReloadProxies(IRunnerMessaging obj)
        {
            // Set Proxies
            if (ProxyFile == null) return;
            var proxies = File.ReadLines(ProxyFile)
                    .Select(p => new CProxy(p, ProxyType))
                    .ToList();

            List<CProxy> toAdd;
            if (Runner.Config.Settings.OnlySocks) toAdd = proxies.Where(x => x.Type != ProxyType.Http).ToList();
            else if (Runner.Config.Settings.OnlySsl) toAdd = proxies.Where(x => x.Type == ProxyType.Http).ToList();
            else toAdd = proxies;

            Runner.ProxyPool = new ProxyPool(toAdd);
                
        }

        private static void MessageArrived(IRunnerMessaging obj, LogLevel level, string message, bool prompt, int timeout)
        {
            // Do not print anything if no verbose argument was declared
            if (!Verbose) return;

            // Select the line color based on the level
            Color color = Color.White;
            switch (level)
            {
                case LogLevel.Warning:
                    color = Color.Orange;
                    break;

                case LogLevel.Error:
                    color = Color.Tomato;
                    break;
            }

            // Print the message to the screen
            Console.WriteLine($"[{DateTime.Now}][{level}] {message}", color);
        }

        private static void FoundHit(IRunnerMessaging obj, Hit hit)
        {
            // Print the hit information to the screen
            Console.WriteLine($"[{DateTime.Now}][{hit.Type}][{hit.Proxy}] {hit.Data}", Color.GreenYellow);

            // If an output file was specified, print them to the output file as well
            if (outFile != string.Empty)
            {
                lock (FileLocker.GetLock(outFile))
                {
                    File.AppendAllText(outFile, $"[{ DateTime.Now}][{hit.Type}][{hit.Proxy}] {hit.Data}{Environment.NewLine}");
                }
            }
        }

        private static void DispatchAction(IRunnerMessaging obj, Action action)
        {
            // No need to delegate the action to the UI thread in CLI, so just invoke it
            action.Invoke();
        }

        private static void AskCustomInputs(IRunnerMessaging obj)
        {
            // Ask all the custom inputs in the console and set their values in the Runner
            foreach (var input in Runner.Config.Settings.CustomInputs)
            {
                Console.WriteLine($"Set custom input ({input.Description}): ", Color.Aquamarine);
                Runner.CustomInputs.Add(new KeyValuePair<string, string>(input.VariableName, Console.ReadLine()));
            }
        }

        private static void Run(Options opts)
        {
            // Set Runner settings from the specified Options
            LoadOptions(opts);

            // Create the hits file
            if (opts.OutputFile != string.Empty)
            {
                File.Create(outFile).Close();
                Console.WriteLine($"The hits file is {outFile}", Color.Aquamarine);
            }

            // Start the runner
            Runner.Start();

            // Wait until it finished
            while (Runner.Busy)
            {
                Thread.Sleep(100);
                UpdateTitle();
            }

            // Print colored finish message
            Console.Write($"Finished. Found: ");
            Console.Write($"{Runner.HitCount} hits, ", Color.GreenYellow);
            Console.Write($"{Runner.CustomCount} custom, ", Color.DarkOrange);
            Console.WriteLine($"{Runner.ToCheckCount} to check.", Color.Aquamarine);

            // Prevent console from closing until the user presses return, then close
            Console.ReadLine();
            Environment.Exit(0);
        }

        private static void HandleParseError(IEnumerable<Error> errs)
        {
            
        }

        private static void LoadOptions(Options opts)
        {
            // Load the user-defined options into local variables or into the local Runner instance
            Verbose = opts.Verbose;
            outFile = opts.OutputFile;
            ProxyFile = opts.ProxyFile;
            ProxyType = opts.ProxyType;
            Runner.SetConfig(IOManager.LoadConfig(opts.ConfigFile), false);
            Runner.SetWordlist(new Wordlist(opts.WordlistFile, opts.WordlistFile, opts.WordlistType, ""));
            Runner.StartingPoint = opts.Skip;
            if (opts.BotsNumber <= 0) Runner.BotsAmount = Runner.Config.Settings.SuggestedBots;
            else Runner.BotsAmount = opts.BotsNumber;

            if (opts.ProxyFile != null && opts.UseProxies != null)
            {
                Runner.ProxyMode = (bool)opts.UseProxies ? ProxyMode.On : ProxyMode.Off;
            }
        }

        private static void LogErrorAndExit(string message)
        {
            Console.WriteLine($"ERROR: {message}", Color.Tomato);
            Console.ReadLine();
            Environment.Exit(0);
        }

        private static void UpdateTitle()
        {
            Console.Title = $"OpenBulletCLI - {Runner.Master.Status} | " +
                $"Config: {Runner.ConfigName} | " +
                $"Wordlist {Runner.WordlistName} | " +
                $"Bots {Runner.BotsAmount} | " +
                $"CPM: {Runner.CPM} | " +
                $"Progress: {Runner.ProgressCount} / {Runner.WordlistSize} ({Runner.Progress}%) | " +
                $"Hits: {Runner.HitCount} Custom: {Runner.CustomCount} ToCheck: {Runner.ToCheckCount} Fails: {Runner.FailCount} Retries: {Runner.RetryCount} | " +
                $"Proxies: {Runner.AliveProxiesCount} / {Runner.TotalProxiesCount}";
        }
    }
}
