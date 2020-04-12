using OpenBullet.Plugins;
using RuriLib.ViewModels;
using System;
using System.ComponentModel;
using System.Windows.Controls;

namespace OpenBullet.Views.UserControls
{
    /// <summary>
    /// Logica di interazione per UserControlCheckbox.xaml
    /// </summary>
    public partial class UserControlCheckbox : UserControl, IControl
    {
        private ViewModelBase viewModel;

        public UserControlCheckbox(bool defaultValue, ViewModelBase viewModel = null)
        {
            InitializeComponent();
            DataContext = this;

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
            return valueCheckbox.IsChecked.Value;
        }

        public void SetValue(dynamic value)
        {
            valueCheckbox.IsChecked = (bool)value;
        }
    }
}
