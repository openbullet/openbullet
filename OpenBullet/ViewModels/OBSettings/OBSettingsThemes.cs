using RuriLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenBullet.ViewModels
{
    public class OBSettingsThemes : ViewModelBase
    {
        // BACKGROUND
        private string backgroundMain = "#222";
        public string BackgroundMain { get { return backgroundMain; } set { backgroundMain = value; OnPropertyChanged(); } }
        private string backgroundSecondary = "#111";
        public string BackgroundSecondary { get { return backgroundSecondary; } set { backgroundSecondary = value; OnPropertyChanged(); } }

        // FOREGROUND
        private string foregroundMain = "#dcdcdc"; // Gainsboro
        public string ForegroundMain { get { return foregroundMain; } set { foregroundMain = value; OnPropertyChanged(); } }
        private string foregroundGood = "#adff2f"; // GreenYellow
        public string ForegroundGood { get { return foregroundGood; } set { foregroundGood = value; OnPropertyChanged(); } }
        private string foregroundBad = "#ff6347"; // Tomato
        public string ForegroundBad { get { return foregroundBad; } set { foregroundBad = value; OnPropertyChanged(); } }
        private string foregroundFree = "#ff8c00"; // DarkOrange
        public string ForegroundCustom { get { return foregroundFree; } set { foregroundFree = value; OnPropertyChanged(); } }
        private string foregroundRetry = "#ffff00"; // Yellow
        public string ForegroundRetry { get { return foregroundRetry; } set { foregroundRetry = value; OnPropertyChanged(); } }
        private string foregroundToCheck = "#7fffd4"; // Aquamarine
        public string ForegroundToCheck { get { return foregroundToCheck; } set { foregroundToCheck = value; OnPropertyChanged(); } }
        private string foregroundMenuSelected = "#1e90ff";
        public string ForegroundMenuSelected { get { return foregroundMenuSelected; } set { foregroundMenuSelected = value; OnPropertyChanged(); } }

        // IMAGES
        private bool useImage = false;
        public bool UseImage { get { return useImage; } set { useImage = value; OnPropertyChanged(); } }
        private string backgroundImage = "";
        public string BackgroundImage { get { return backgroundImage; } set { backgroundImage = value; OnPropertyChanged(); } }
        private int backgroundImageOpacity = 100;
        public int BackgroundImageOpacity { get { return backgroundImageOpacity; } set { backgroundImageOpacity = value; OnPropertyChanged(); } }
        private string backgroundLogo = "";
        public string BackgroundLogo { get { return backgroundLogo; } set { backgroundLogo = value; OnPropertyChanged(); } }

        private bool enableSnow = false;
        public bool EnableSnow { get { return enableSnow; } set { enableSnow = value; OnPropertyChanged(); } }
        private int snowAmount = 100;
        public int SnowAmount { get { return snowAmount; } set { snowAmount = value; OnPropertyChanged(); } }
        private bool allowTransparency = false;
        public bool AllowTransparency { get { return allowTransparency; } set { allowTransparency = value; OnPropertyChanged(); } }
    }
}
