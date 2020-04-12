using System;
using System.ComponentModel;
using OpenBullet.Plugins;
using RuriLib.ViewModels;

namespace OpenBullet.Views.UserControls
{
    /// <summary>
    /// Logica di interazione per UserControlText.xaml
    /// </summary>
    public partial class UserControlText : IControl
    {
        private ViewModelBase viewModel;

        public UserControlText(string defaultValue, bool readOnly = false, ViewModelBase viewModel = null)
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
            return valueTextbox.Text;
        }

        public void SetValue(dynamic value)
        {
            valueTextbox.Text = (string)value;
        }
    }
}
