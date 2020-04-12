using OpenBullet.Plugins;
using RuriLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OpenBullet.Views.UserControls
{
    /// <summary>
    /// Logica di interazione per UserControlInfoText.xaml
    /// </summary>
    public partial class UserControlInfoText : IControl
    {
        private ViewModelBase viewModel;

        public UserControlInfoText(string defaultValue, ViewModelBase viewModel = null)
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

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SetValue(viewModel.GetType().GetProperty(e.PropertyName).GetValue(viewModel));
        }

        public dynamic GetValue()
        {
            return valueLabel.Content;
        }

        public void SetValue(dynamic value)
        {
            valueLabel.Content = (string)value;
        }
    }
}
