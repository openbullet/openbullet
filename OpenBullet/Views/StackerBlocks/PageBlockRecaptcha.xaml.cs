using RuriLib;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet.Pages.StackerBlocks
{
    /// <summary>
    /// Logica di interazione per PageBlockRecaptcha.xaml
    /// </summary>
    public partial class PageBlockRecaptcha : Page
    {
        BlockRecaptcha vm;

        public PageBlockRecaptcha(BlockRecaptcha block)
        {
            InitializeComponent();
            vm = block;
            DataContext = vm;
        }

        private void autoSiteKey_Click(object sender, RoutedEventArgs e)
        {
            if (vm.Url == "") { MessageBox.Show("You cannot use auto without setting a page where the reCaptcha is shown first!"); return; }
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(vm.Url);
                request.AutomaticDecompression = DecompressionMethods.GZip;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    var html = reader.ReadToEnd();
                    Regex r = new Regex("[^A-Za-z\\d][A-Za-z\\d\\-]{40}[^A-Za-z\\d]");
                    vm.SiteKey = r.Match(html).Value.Replace("\"", "");
                }
            }
            catch { MessageBox.Show("Auto failed. Do it manually"); return; }
        }
    }
}
