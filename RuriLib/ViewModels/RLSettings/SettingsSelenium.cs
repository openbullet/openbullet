using System.Collections.Generic;
using System.Reflection;

namespace RuriLib.ViewModels
{
    /// <summary>
    /// The Type of supported Browser to drive via selenium.
    /// </summary>
    public enum BrowserType
    {
        /// <summary>Google Chrome</summary>
        Chrome,
        /// <summary>Mozilla Firefox</summary>
        Firefox
    }

    /// <summary>
    /// Provides selenium-related settings.
    /// </summary>
    public class SettingsSelenium : ViewModelBase
    {
        private BrowserType browser = BrowserType.Chrome;
        /// <summary>The Browser to be used.</summary>
        public BrowserType Browser { get { return browser; } set { browser = value; OnPropertyChanged(); } }

        private bool headless = false;
        /// <summary>Whether to run the browser in Headless mode (--headless argument).</summary>
        public bool Headless { get { return headless; } set { headless = value; OnPropertyChanged(); } }

        private string firefoxBinaryLocation = @"C:\Program Files (x86)\Mozilla Firefox\firefox.exe";
        /// <summary>The location of the firefox.exe binary on disk.</summary>
        public string FirefoxBinaryLocation { get { return firefoxBinaryLocation; } set { firefoxBinaryLocation = value; OnPropertyChanged(); } }

        private string chromeBinaryLocation = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe";
        /// <summary>The location of the chrome.exe binary on disk.</summary>
        public string ChromeBinaryLocation { get { return chromeBinaryLocation; } set { chromeBinaryLocation = value; OnPropertyChanged(); } }

        private List<string> chromeExtensions = new List<string>();
        /// <summary>The list of .crx extensions for Chrome inside the ChromeExtensions folder. Extensions don't work in Headless mode!</summary>
        public List<string> ChromeExtensions { get { return chromeExtensions; } set { chromeExtensions = value; OnPropertyChanged(); } }

        private bool drawMouseMovement = true;
        /// <summary>Whether to draw red dots to follow the mouse movement when a MOUSEACTION command is issued.</summary>
        public bool DrawMouseMovement { get { return drawMouseMovement; } set { drawMouseMovement = value; OnPropertyChanged(); } }

        private int pageLoadTimeout = 60;
        /// <summary>The default timeout for page load in the browser.</summary>
        public int PageLoadTimeout { get { return pageLoadTimeout; } set { pageLoadTimeout = value; OnPropertyChanged(); } }

        /// <summary>
        /// Resets the properties to their default value.
        /// </summary>
        public void Reset()
        {
            SettingsSelenium def = new SettingsSelenium();
            IList<PropertyInfo> props = new List<PropertyInfo>(typeof(SettingsSelenium).GetProperties());

            foreach (PropertyInfo prop in props)
            {
                prop.SetValue(this, prop.GetValue(def, null));
            }
        }
    }
}
