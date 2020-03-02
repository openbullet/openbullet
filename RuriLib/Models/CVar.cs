using System;
using System.Collections.Generic;
using System.Linq;

namespace RuriLib.Models
{
    /// <summary>
    /// Represents a variable.
    /// </summary>
    public class CVar
    {
        /// <summary>
        /// The type of variable.
        /// </summary>
        public enum VarType
        {
            /// <summary>Holds a single string as value.</summary>
            Single,

            /// <summary>Holds a list of strings as value.</summary>
            List,

            /// <summary>Holds a dictionary of strings as value.</summary>
            Dictionary
        }

        /// <summary>The variable name.</summary>
        public string Name { get; set; }

        /// <summary>The dynamic variable value.</summary>
        public dynamic Value { get; set; }

        /// <summary>Whether the variable is used for the final capture.</summary>
        public bool IsCapture { get; set; }

        /// <summary>The variable type (it determines which type of value is expected).</summary>
        public VarType Type { get; set; }

        /// <summary>Whether the variable is hidden and shouldn't be displayed to the user.</summary>
        public bool Hidden { get; set; }

        // Constructors
        /// <summary>Needed for NoSQL deserialization.</summary>
        public CVar() { }

        /// <summary>
        /// Creates a variable of type Single.
        /// </summary>
        /// <param name="name">The variable name</param>
        /// <param name="value">The variable value as a single string</param>
        /// <param name="isCapture">Whether the variable is marked as Capture</param>
        /// <param name="hidden">Whether the variable is hidden</param>
        public CVar(string name, string value, bool isCapture = false, bool hidden = false)
        {
            Name = name;
            Value = value;
            IsCapture = isCapture;
            Type = VarType.Single;
            Hidden = hidden;
        }

        /// <summary>
        /// Creates a variable of type List.
        /// </summary>
        /// <param name="name">The variable name</param>
        /// <param name="value">The variable value as a list of strings</param>
        /// <param name="isCapture">Whether the variable is marked as Capture</param>
        /// <param name="hidden">Whether the variable is hidden</param>
        public CVar(string name, List<string> value, bool isCapture = false, bool hidden = false)
        {
            Name = name;
            Value = value;
            IsCapture = isCapture;
            Type = VarType.List;
            Hidden = hidden;
        }

        /// <summary>
        /// Creates a variable of type Dictionary.
        /// </summary>
        /// <param name="name">The variable name</param>
        /// <param name="value">The variable value as a dictionary of strings</param>
        /// <param name="isCapture">Whether the variable is marked as Capture</param>
        /// <param name="hidden">Whether the variable is hidden</param>
        public CVar(string name, Dictionary<string, string> value, bool isCapture = false, bool hidden = false)
        {
            Name = name;
            Value = value;
            IsCapture = isCapture;
            Type = VarType.Dictionary;
            Hidden = hidden;
        }

        /// <summary>
        /// Creates a variable of a given type.
        /// </summary>
        /// <param name="name">The variable name</param>
        /// <param name="type">The variable type</param>
        /// <param name="value">The dynamic variable value</param>
        /// <param name="isCapture">Whether the variable is marked as Capture</param>
        /// <param name="hidden">Whether the variable is hidden</param>
        public CVar(string name, VarType type, dynamic value, bool isCapture = false, bool hidden = false)
        {
            Name = name;
            Value = value;
            IsCapture = isCapture;
            Type = type;
            Hidden = hidden;
        }

        /// <summary>
        /// Outputs a string from a variable.
        /// </summary>
        /// <returns>The formatted value of the value as a single string</returns>
        public override string ToString()
        {
            switch (Type)
            {
                case VarType.Single:
                    return Value;

                case VarType.List:
                    if (Value.GetType() == typeof(List<string>)) return "[" + string.Join(", ", (List<string>)Value) + "]"; // Coming internally
                    else if (Value.GetType() == typeof(object[])) return "[" + string.Join(", ", Value) + "]"; // Coming from the DB
                    else return "";

                case VarType.Dictionary:
                    return "{" + string.Join(", ", ((Dictionary<string, string>)Value).Select(d => "(" + d.Key + ", " + d.Value + ")")) + "}";

                default:
                    return base.ToString();
            }
        }

        /// <summary>
        /// Gets an item from a List variable.
        /// </summary>
        /// <param name="index">The index of the item</param>
        /// <returns>The item of the List variable's value</returns>
        public string GetListItem(int index)
        {
            if (Type != VarType.List) return null;
            var list = Value as List<string>;

            // If the index is negative, start from the end
            if (index < 0)
            {
                // For example in a [1,2,3] list, the element at -1 is at index 3-1 = 2 which is element '3'
                index = list.Count + index;
            }

            if (index > list.Count - 1 || index < 0) return null;
            return list[index];
        }

        /// <summary>
        /// Gets an item from a Dictionary Key.
        /// </summary>
        /// <param name="key">The key of the dictionary entry</param>
        /// <returns>The value of the Dictionary variable's entry</returns>
        public string GetDictValue(string key)
        {
            var dict = Value as Dictionary<string, string>;
            if (dict.ContainsKey(key)) return dict.First(d => d.Key == key).Value;
            else throw new Exception("Key not in dictionary");
        }

        /// <summary>
        /// Gets an item from a Dictionary Value.
        /// </summary>
        /// <param name="value">The value of the dictionary entry</param>
        /// <returns>The key of the Dictionary variable's entry</returns>
        public string GetDictKey(string value)
        {
            var dict = Value as Dictionary<string, string>;
            if (dict.ContainsValue(value)) return dict.First(d => d.Value == value).Key;
            else throw new Exception("Value not in dictionary");
        }
    }
}
