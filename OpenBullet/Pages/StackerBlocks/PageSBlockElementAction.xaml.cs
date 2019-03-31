using RuriLib;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace OpenBullet.Pages.StackerBlocks
{
    /// <summary>
    /// Logica di interazione per PageSBlockElementAction.xaml
    /// </summary>
    public partial class PageSBlockElementAction : Page
    {
        SBlockElementAction vm;

        public PageSBlockElementAction(SBlockElementAction block)
        {
            InitializeComponent();
            vm = block;
            DataContext = vm;

            foreach (var l in Enum.GetNames(typeof(ElementLocator)))
                locatorCombobox.Items.Add(l);

            locatorCombobox.SelectedIndex = (int)vm.Locator;

            foreach (var a in Enum.GetNames(typeof(ElementAction)))
                actionCombobox.Items.Add(a);

            actionCombobox.SelectedIndex = (int)vm.Action;
        }

        private void locatorCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.Locator = (ElementLocator)((ComboBox)e.OriginalSource).SelectedIndex;
        }

        private void actionCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.Action = (ElementAction)((ComboBox)e.OriginalSource).SelectedIndex;
            try { functionInfoTextblock.Text = infoDic[vm.Action.ToString()]; } catch { functionInfoTextblock.Text = "No additional information available for this function"; }
        }

        public Dictionary<string, string> infoDic = new Dictionary<string, string>()
        {
            { "Clear", "Clears the text in the targeted input element." },
            { "SendKeys", "Sends the text to the targeted input element." },
            { "Submit", "Submits the targeted form." },
            { "GetText", "Gets the innerHTML of the targeted element." },
            { "GetAttribute", "From the targeted element, gets the attribute with the name specified in the input field." },
            { "Screenshot", "Takes a screenshot of the targeted element, you can use this to screenshot a gift captcha and send it to be solved later with a Captcha Block." },
            { "WaitForElement", "Waits until the element specified exists. You can set the timeout in seconds through the input field [Default Timeout = 10s]." }
        };
    }
}
