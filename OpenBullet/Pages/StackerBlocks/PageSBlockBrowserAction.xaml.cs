using RuriLib;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace OpenBullet.Pages.StackerBlocks
{
    /// <summary>
    /// Logica di interazione per PageSBlockBrowserAction.xaml
    /// </summary>
    public partial class PageSBlockBrowserAction : Page
    {
        SBlockBrowserAction vm;

        public PageSBlockBrowserAction(SBlockBrowserAction block)
        {
            InitializeComponent();
            vm = block;
            DataContext = vm;

            foreach (var a in Enum.GetNames(typeof(BrowserAction)))
                actionCombobox.Items.Add(a);

            actionCombobox.SelectedIndex = (int)vm.Action;
        }

        private void actionCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.Action = (BrowserAction)((ComboBox)e.OriginalSource).SelectedIndex;
            try { functionInfoTextblock.Text = infoDic[vm.Action.ToString()]; } catch { functionInfoTextblock.Text = "No additional information available for this function"; }
        }

        public Dictionary<string, string> infoDic = new Dictionary<string, string>()
        {
            { "Open", "Opens the browser assigned to the current bot. This will be disregarded if the browser is already opened." },
            { "Close", "Closes the current tab without disposing of the webdriver (not recommended)." },
            { "Quit", "Quits all the tabs and windows and disposes of the webdriver (recommended)." },
            { "SendKeys", "Sends the keys directly to the browser as if you were typing them on your keyboard. You can use variables and <TAB> <ENTER> and <BACKSPACE> separated by || e.g. <USER>||<TAB>||<PASS>||<TAB>||<ENTER>." },
            { "MoveMouse", "Not Implemented Yet" },
            { "Screenshot", "Takes a screenshot of the visible part of the page." },
            { "Scroll", "Scrolls down by the specified amount in the input field." },
            { "SwitchToTab", "Switches to the tab with the index in the input field." },
            { "DOMtoSOURCE", "Puts the full DOM into the <SOURCE> fixed variable." },
            { "GetCookies", "Sends all cookies from selenium to the cookie jar used in normal requests." },
            { "SetCookies", "Sets all cookies from the cookie jar to selenium. You need to put the domain of the website in the input field (e.g. example.com)." }
        };
    }
}
