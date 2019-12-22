using PluginFramework;
using PluginFramework.Attributes;
using RuriLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OpenBullet.Plugins
{
    public static class Check
    {
        private static Dictionary<Type, Type> _requiredPropertyTypes = new Dictionary<Type, Type>()
        {
            { typeof(Text), typeof(string) },
            { typeof(Numeric), typeof(int) },
            { typeof(Checkbox), typeof(bool) },
            { typeof(TextMulti), typeof(string[]) },
            { typeof(FilePicker), typeof(string) },
            { typeof(Dropdown), typeof(string) }
        };

        public static bool InputProperty(PropertyInfo property)
        {
            // Check if it has one and only one InputField
            if (property.GetCustomAttributes().Count(a => a is InputField) != 1)
            {
                return false;
            }

            /*
            * HERE we know for certain that the property was meant to be used as an input field, so we will start throwing exceptions
            * if there is something that doesn't meet the criteria
            */

            // Get the first attribute
            var attribute = property.GetCustomAttribute<InputField>();

            // Check if the attribute type exists in the dictionary
            if (!_requiredPropertyTypes.ContainsKey(attribute.GetType()))
            {
                throw new Exception($"Unknown attribute type {attribute.GetType()}");
            }

            var requiredType = _requiredPropertyTypes[attribute.GetType()];

            // Check if the property type is correct
            if (property.PropertyType != requiredType)
            {
                throw new Exception($"The property {property.Name} must be of type {requiredType}");
            }

            return true;
        }

        public static bool Method(IPlugin plugin, MethodInfo method)
        {
            // Check if it has one and only one Button attribute
            if (method.GetCustomAttributes().Count(a => a is Button) != 1)
            {
                return false;
            }

            /*
            * HERE we know for certain that the method was meant to be used as a button, so we will start throwing exceptions
            * if there is something that doesn't meet the criteria
            */

            // Get the first attribute
            var attribute = method.GetCustomAttribute<Button>();
            var parameters = method.GetParameters();

            // Check if it accepts 0 or 1 parameters
            if (parameters.Length > 1)
            {
                return false;
            }

            // If the method accepts 1 parameter, check that its type is IApplication
            if (parameters.Length == 1 && !parameters.Any(p => p.ParameterType == typeof(IApplication)))
            {
                return false;
            }

            return true;
        }
    }
}
