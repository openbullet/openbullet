using OpenBullet.Views.UserControls;
using PluginFramework;
using PluginFramework.Attributes;
using RuriLib.ViewModels;
using System;
using System.Reflection;

namespace OpenBullet.Plugins
{
    public static class Build
    {
        public static UserControlContainer InputField(object plugin, PropertyInfo property, ViewModelBase viewModel = null)
        {
            // Get the first attribute
            var attribute = property.GetCustomAttribute<InputField>();

            IControl control;
            var defaultValue = (dynamic)Convert.ChangeType(property.GetValue(plugin), property.PropertyType);

            switch (attribute)
            {
                case InfoText a:
                    control = new UserControlInfoText(defaultValue, viewModel);
                    break;

                case Text a:
                    control = new UserControlText(defaultValue, a.readOnly, viewModel);
                    break;

                case Numeric a:
                    control = new UserControlNumeric(defaultValue, a.minimum, a.maximum, viewModel);
                    break;

                case Checkbox a:
                    control = new UserControlCheckbox(defaultValue, viewModel);
                    break;

                case TextMulti a:
                    control = new UserControlTextMulti(defaultValue, a.readOnly, viewModel);
                    break;

                case FilePicker a:
                    control = new UserControlFilePicker(defaultValue, a.filter);
                    break;

                case Dropdown a:
                    control = new UserControlDropdown(defaultValue, a.options, viewModel);
                    break;

                case WordlistPicker a:
                    control = new UserControlWordlist();
                    break;

                case ConfigPicker a:
                    control = new UserControlConfig();
                    break;

                default:
                    throw new NotImplementedException();
            }

            return new UserControlContainer(property.Name, control, attribute.label, attribute.tooltip);
        }

        public static ButtonContainer Button(MethodInfo method, PluginControl pluginControl)
        {
            // Get the first attribute
            var attribute = method.GetCustomAttribute<Button>();

            return new ButtonContainer(attribute.text, method.Name, pluginControl);
        }
    }
}
