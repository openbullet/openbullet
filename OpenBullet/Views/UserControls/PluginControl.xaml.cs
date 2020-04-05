using OpenBullet.Plugins;
using PluginFramework;
using RuriLib.Interfaces;
using RuriLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;

namespace OpenBullet.Views.UserControls
{
    /// <summary>
    /// Logica di interazione per PluginControl.xaml
    /// </summary>
    public partial class PluginControl : UserControl
    {
        public IPlugin Plugin { get; set; }
        public ObservableCollection<UserControl> Controls { get; set; } = new ObservableCollection<UserControl>();
        private List<PropertyInfo> ValidProperties { get; set; } = new List<PropertyInfo>();

        public PluginControl(Type type, IApplication app, bool supportsPropertyChanged = false)
        {
            InitializeComponent();
            DataContext = this;

            var constructorParams = new object[] { };

            // If the constructor supports IApplication
            if (type.GetConstructors()
                .Any(c => c.GetParameters()
                    .Any(p => p.ParameterType == typeof(IApplication))))
            {
                constructorParams = new object[] { app };
            }
            
            Plugin = Activator.CreateInstance(type, constructorParams) as IPlugin;

            // For each valid property, add input field
            foreach (var p in type.GetProperties().Where(p => Check.InputProperty(p)))
            {
                ValidProperties.Add(p);
                Controls.Add(Build.InputField(Plugin, p,
                    supportsPropertyChanged ? (Plugin as ViewModelBase) : null));
            }

            // For each valid method, add button
            foreach (var m in type.GetMethods().Where(m => Check.Method(Plugin, m)))
            {
                Controls.Add(Build.Button(m, this));
            }
        }

        public void RunMethod(string methodName)
        {
            foreach (var property in ValidProperties)
            {
                // Retrieve and set the value
                property.SetValue(Plugin, Controls
                    .Where(c => c is UserControlContainer)
                    .Select(c => c as UserControlContainer)
                    .First(c => c.PropertyName == property.Name).GetValue());
            }

            var method = Plugin.GetType().GetMethod(methodName);
            var parameters = method.GetParameters();
            var passed = new object[] { };

            // If the method supports 1 paramters of type IApplication
            if (parameters.Length == 1 && parameters.First().ParameterType == typeof(IApplication))
            {
                passed = new object[] { OB.App };
            }

            method.Invoke(Plugin, passed);
        }
    }
}
