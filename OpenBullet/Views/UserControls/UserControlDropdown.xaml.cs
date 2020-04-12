using OpenBullet.Plugins;
using RuriLib.ViewModels;
using System;
using System.ComponentModel;
using System.Windows.Controls;

namespace OpenBullet.Views.UserControls
{
    /// <summary>
    /// Logica di interazione per UserControlDropdown.xaml
    /// </summary>
    public partial class UserControlDropdown : UserControl, IControl
    {
        private ViewModelBase viewModel;

        public UserControlDropdown(string value, string[] options, ViewModelBase viewModel = null)
        {
            InitializeComponent();
            DataContext = this;

            foreach (var option in options)
            {
                valueDropdown.Items.Add(option);
            }

            SetValue(value);

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
            return valueDropdown.SelectedValue;
        }

        public void SetValue(dynamic value)
        {
            valueDropdown.SelectedValue = (string)value;
        }
    }
}
