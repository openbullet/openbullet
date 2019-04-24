using Newtonsoft.Json;

namespace RuriLib.ViewModels
{
    /// <summary>
    /// An observable wrapper around a Config object.
    /// </summary>
    public class ConfigViewModel : ViewModelBase
    {
        /// <summary>The actual Config object.</summary>
        public Config Config { get; set; }
        
        private string category = "Default";
        /// <summary>The Category of the config.</summary>
        public string Category { get { return category; } set { category = value; OnPropertyChanged(); } }

        private string path = "";
        /// <summary>The path of the config file on disk.</summary>
        public string Path { get { return path; } set { path = value; OnPropertyChanged(); } }

        /// <summary>Whether the config was pulled from a remote source.</summary>
        public bool Remote { get; set; } = false;

        /// <summary>The name of the config.</summary>
        public string Name { get { return Config.Settings.Name; } }

        /// <summary>
        /// Constructs an instance of the ConfigViewModel class.
        /// </summary>
        /// <param name="path">The path of the config file on disk</param>
        /// <param name="category">The category of the config</param>
        /// <param name="config">The actual Config object</param>
        /// <param name="remote">Whether the Config was pulled from a remote source</param>
        public ConfigViewModel(string path, string category, Config config, bool remote = false)
        {
            Path = path;
            Category = category;
            Config = config;
            Remote = remote;
        }
    }
}
