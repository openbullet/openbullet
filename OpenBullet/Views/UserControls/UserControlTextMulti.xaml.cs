using OpenBullet.Plugins;
using RuriLib.ViewModels;
using System;
using System.ComponentModel;
using System.Windows.Controls;

namespace OpenBullet.Views.UserControls
{
    /// <summary>
    /// Logica di interazione per UserControlTextMulti.xaml
    /// </summary>
    public partial class UserControlTextMulti : UserControl, IControl
    {
        private ViewModelBase viewModel;

        public UserControlTextMulti(string[] defaultValue, bool readOnly = false, ViewModelBase viewModel = null)
        {
            InitializeComponent();
            DataContext = this;
            valueTextbox.IsReadOnly = readOnly;

            SetValue(defaultValue);

            this.viewModel = viewModel;
            if (viewModel != null)
            {
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SetValue(viewModel.GetType().GetProperty(e.PropertyName).GetValue(viewModel));
        }

        public dynamic GetValue()
        {
            return valueTextbox.Text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
        }

        public void SetValue(dynamic value)
        {
            var val = (string[])value;
            valueTextbox.Text = string.Join(Environment.NewLine, val);
        }
    }
}
