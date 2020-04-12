using OpenBullet.Plugins;
using RuriLib.ViewModels;
using System;
using System.ComponentModel;
using System.Windows.Controls;

namespace OpenBullet.Views.UserControls
{
    /// <summary>
    /// Logica di interazione per UserControlNumeric.xaml
    /// </summary>
    public partial class UserControlNumeric : UserControl, IControl
    {
        private ViewModelBase viewModel;

        public int Minimum { get; set; }
        public int Maximum { get; set; }

        public UserControlNumeric(int defaultValue, int minimum, int maximum, ViewModelBase viewModel = null)
        {
            InitializeComponent();
            DataContext = this;

            Minimum = minimum;
            Maximum = maximum;

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
            return valueNumeric.Value;
        }

        public void SetValue(dynamic value)
        {
            valueNumeric.Value = (int)value;
        }
    }
}
