using System;
using System.Collections.Generic;

namespace RuriLib
{
    /// <summary>
    /// Allows to make a switch statement on the Type of the object, and executes a different Action depending on its outcome.
    /// </summary>
    public class TypeSwitch
    {
        Dictionary<Type, Action<object>> matches = new Dictionary<Type, Action<object>>();

        /// <summary>
        /// Adds a case to the TypeSwitch.
        /// </summary>
        /// <typeparam name="T">The type</typeparam>
        /// <param name="action">The action to perform</param>
        /// <returns>The TypeSwitch itself</returns>
        public TypeSwitch Case<T>(Action<T> action) { matches.Add(typeof(T), (x) => action((T)x)); return this; }

        /// <summary>
        /// Runs the switch statement on an object and executes the corresponding Action.
        /// </summary>
        /// <param name="x">The object on which you want to execute the switch statement</param>
        public void Switch(object x) { matches[x.GetType()](x); }
    }
}
