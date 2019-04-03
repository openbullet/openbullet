using RuriLib;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace OpenBullet.Pages.StackerBlocks
{
    /// <summary>
    /// Logica di interazione per PageBlockRequest.xaml
    /// </summary>
    public partial class PageBlockRequest : Page
    {
        BlockRequest vm;

        public PageBlockRequest(BlockRequest block)
        {
            InitializeComponent();
            vm = block;
            DataContext = vm;

            foreach (string i in Enum.GetNames(typeof(Extreme.Net.HttpMethod)))
                methodCombobox.Items.Add(i);

            methodCombobox.SelectedIndex = (int)vm.Method;

            foreach (string t in Enum.GetNames(typeof(RequestType)))
                requestTypeCombobox.Items.Add(t);

            requestTypeCombobox.SelectedIndex = (int)vm.RequestType;

            foreach (string t in Enum.GetNames(typeof(ResponseType)))
                responseTypeCombobox.Items.Add(t);

            responseTypeCombobox.SelectedIndex = (int)vm.ResponseType;

            customCookiesRTB.AppendText(vm.GetCustomCookies());
            customHeadersRTB.AppendText(vm.GetCustomHeaders());
            multipartContentsRTB.AppendText(vm.GetMultipartContents());

            List<string> commonContentTypes = new List<string>()
            {
                "application/x-www-form-urlencoded",
                "application/json",
                "text/plain"
            };

            foreach (var c in commonContentTypes)
                contentTypeCombobox.Items.Add(c);
        }

        private void methodCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.Method = (Extreme.Net.HttpMethod)methodCombobox.SelectedIndex;
        }
        
        private void requestTypeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.RequestType = (RequestType)requestTypeCombobox.SelectedIndex;

            switch (vm.RequestType)
            {
                default:
                    requestTypeTabControl.SelectedIndex = 1;
                    break;

                case RequestType.Standard:
                    requestTypeTabControl.SelectedIndex = 2;
                    break;

                case RequestType.Multipart:
                    requestTypeTabControl.SelectedIndex = 3;
                    break;
            }
        }

        private void responseTypeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.ResponseType = (ResponseType)responseTypeCombobox.SelectedIndex;

            switch (vm.ResponseType)
            {
                default:
                    responseTypeTabControl.SelectedIndex = 0;
                    break;

                case ResponseType.File:
                    responseTypeTabControl.SelectedIndex = 1;
                    break;
            }
        }

        private void customCookiesRTB_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            vm.SetCustomCookies(customCookiesRTB.Lines());
        }

        private void customHeadersRTB_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            vm.SetCustomHeaders(customHeadersRTB.Lines());
        }

        private void multipartContentsRTB_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            vm.SetMultipartContents(multipartContentsRTB.Lines());
        }
    }
}
