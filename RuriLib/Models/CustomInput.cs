using Newtonsoft.Json;
using System;

namespace RuriLib.ViewModels
{
    /// <summary>
    /// A custom input that is required to the user before the Config is started.
    /// </summary>
    public class CustomInput : ViewModelBase
    {
        private string description = "";
        /// <summary>The description of what the user needs to enter.</summary>
        public string Description { get { return description; } set { description = value; OnPropertyChanged(); } }

        private string variableName = "";
        /// <summary>The name of the variable to create basing on the value provided by the user.</summary>
        public string VariableName { get { return variableName; } set { variableName = value; OnPropertyChanged(); } }

        /// <summary>The id of the Custom Input.</summary>
        public int Id { get; set; }

        /// <summary>
        /// Creates a CustomInput given an id.
        /// </summary>
        /// <param name="id">A unique id</param>
        public CustomInput(int id)
        {
            Id = id;
        }
    }
}
