using RuriLib;
using RuriLib.Functions.Requests;
using System;
using System.Windows.Controls;

namespace OpenBullet.Views.StackerBlocks
{
    /// <summary>
    /// Logica di interazione per PageBlockBypassCF.xaml
    /// </summary>
    public partial class PageBlockBypassCF : Page
    {
        BlockBypassCF vm;

        public PageBlockBypassCF(BlockBypassCF block)
        {
            InitializeComponent();
            vm = block;
            DataContext = vm;

            foreach (var s in Enum.GetNames(typeof(SecurityProtocol)))
                securityProtocolCombobox.Items.Add(s);

            securityProtocolCombobox.SelectedIndex = (int)vm.SecurityProtocol;
        }

        private void securityProtocolCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.SecurityProtocol = (SecurityProtocol)((ComboBox)e.OriginalSource).SelectedIndex;
        }
    }
}
