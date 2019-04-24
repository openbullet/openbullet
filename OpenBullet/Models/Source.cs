using RuriLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenBullet.Models
{
    public class Source : ViewModelBase
    {
        public enum AuthMode
        {
            ApiKey,
            UserPass
        }

        private string apiUrl = "";
        public string ApiUrl { get { return apiUrl; } set { apiUrl = value; OnPropertyChanged(); } }

        private AuthMode auth = AuthMode.ApiKey;
        public AuthMode Auth { get { return auth; } set { auth = value; OnPropertyChanged(); } }

        private string apiKey = "";
        public string ApiKey { get { return apiKey; } set { apiKey = value; OnPropertyChanged(); } }

        private string username = "";
        public string Username { get { return username; } set { username = value; OnPropertyChanged(); } }

        private string password = "";
        public string Password { get { return password; } set { password = value; OnPropertyChanged(); } }
    }
}
