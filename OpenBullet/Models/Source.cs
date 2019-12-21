using Newtonsoft.Json;
using RuriLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OpenBullet.Models
{
    public class Source : ViewModelBase
    {
        public enum AuthMode
        {
            ApiKey,
            UserPass
        }

        public int Id { get; set; }

        private string apiUrl = "";
        public string ApiUrl { get => apiUrl; set { apiUrl = value; OnPropertyChanged(); } }

        private AuthMode auth = AuthMode.ApiKey;
        public AuthMode Auth
        { 
            get => auth; 
            set
            { 
                auth = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ApiKeyVisible));
                OnPropertyChanged(nameof(UserPassVisible)); 
            } 
        }

        private string apiKey = "";
        public string ApiKey { get => apiKey; set { apiKey = value; OnPropertyChanged(); } }

        private string username = "";
        public string Username { get => username; set { username = value; OnPropertyChanged(); } }

        private string password = "";
        public string Password { get => password; set { password = value; OnPropertyChanged(); } }

        [JsonIgnore]
        public bool AuthInitialized { get; set; } = false;

        [JsonIgnore]
        public Visibility ApiKeyVisible { get => Auth == AuthMode.ApiKey ? Visibility.Visible : Visibility.Collapsed; }

        [JsonIgnore]
        public Visibility UserPassVisible { get => Auth == AuthMode.UserPass ? Visibility.Visible : Visibility.Collapsed; }

        public Source(int id)
        {
            Id = id;
        }
    }
}
