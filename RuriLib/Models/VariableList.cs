using Newtonsoft.Json;
using RuriLib.Functions.Conditions;
using System.Collections.Generic;
using System.Linq;

namespace RuriLib.Models
{
    /// <summary>
    /// Class that allows to easily manage a list of CVar objects.
    /// </summary>
    public class VariableList
    {
        /// <summary>The whole list of variables.</summary>
        public List<CVar> All { get; set; }

        /// <summary>The list of all variables marked as Capture.</summary>
        [JsonIgnore]
        public List<CVar> Captures { get { return All.Where(v => v.IsCapture && !v.Hidden).ToList(); } }

        /// <summary>The list of all variables of type Single.</summary>
        [JsonIgnore]
        public List<CVar> Singles { get { return All.Where(v => v.Type == CVar.VarType.Single).ToList(); } }

        /// <summary>The list of all variables of type List.</summary>
        [JsonIgnore]
        public List<CVar> Lists { get { return All.Where(v => v.Type == CVar.VarType.List).ToList(); } }

        /// <summary>The list of all variables of type Dictionary.</summary>
        [JsonIgnore]
        public List<CVar> Dictionaries { get { return All.Where(v => v.Type == CVar.VarType.Dictionary).ToList(); } }

        /// <summary>
        /// Standard constructor that initializes an empty variable list.
        /// </summary>
        public VariableList()
        {
            All = new List<CVar>();
        }

        /// <summary>
        /// Initializes a variable list starting from an existing list of variables.
        /// </summary>
        /// <param name="list">The list of variables.</param>
        public VariableList(List<CVar> list)
        {
            All = list;
        }
        
        /// <summary>
        /// Gets a variable given its name.
        /// </summary>
        /// <param name="name">The name of the variable</param>
        /// <returns>The variable or null if it wasn't found.</returns>
        public CVar Get(string name)
        {
            return All.FirstOrDefault(v => v.Name == name);
        }

        /// <summary>
        /// Gets a variable given its name and type.
        /// </summary>
        /// <param name="name">The name of the variable</param>
        /// <param name="type">The type of the variable</param>
        /// <returns>The variable or null if it wasn't found.</returns>
        public CVar Get(string name, CVar.VarType type)
        {
            return All.FirstOrDefault(v => v.Name == name && v.Type == type);
        }

        /// <summary>
        /// Helper method that gets the value of a variable of type Single.
        /// </summary>
        /// <param name="name">The name of the Single variable</param>
        /// <returns>The string value or null if it wasn't found.</returns>
        public string GetSingle(string name)
        {
            try { return (string)Get(name, CVar.VarType.Single).Value; }
            catch { return null; }
        }

        /// <summary>
        /// Helper method that gets the value of a variable of type List.
        /// </summary>
        /// <param name="name">The name of the List variable</param>
        /// <returns>The list of strings value or null if it wasn't found.</returns>
        public List<string> GetList(string name)
        {
            try { return (List<string>)Get(name, CVar.VarType.List).Value; }
            catch { return null; }
        }

        /// <summary>
        /// Helper method that gets the value of a variable of type Dictionary.
        /// </summary>
        /// <param name="name">The name of the Dictionary variable</param>
        /// <returns>The dictionary value or null if it wasn't found.</returns>
        public Dictionary<string,string> GetDictionary(string name)
        {
            try { return (Dictionary<string, string>)Get(name, CVar.VarType.Dictionary).Value; }
            catch { return null; }
        }

        /// <summary>
        /// Helper method that checks if a variable exists given its name.
        /// </summary>
        /// <param name="name">The name of the variable</param>
        /// <returns>True if the variable exists</returns>
        public bool VariableExists(string name)
        {
            return All.Any(v => v.Name == name);
        }

        /// <summary>
        /// Helper method that checks if a variable exists given its name and type.
        /// </summary>
        /// <param name="name">The name of the variable</param>
        /// <param name="type">The type of the variable</param>
        /// <returns>True if the variable exists and matches the given type</returns>
        public bool VariableExists(string name, CVar.VarType type)
        {
            return All.Any(v => v.Name == name && v.Type == type);
        }

        /// <summary>
        /// Adds a CVar object to the variable list.
        /// </summary>
        /// <param name="variable">The CVar object to add</param>
        public void Set(CVar variable)
        {
            // First of all remove any old variable with the same name
            Remove(variable.Name);

            // Then add the new one
            All.Add(variable);
        }

        /// <summary>
        /// Adds a hidden CVar object to the variable list.
        /// </summary>
        /// <param name="name">The name of the variable to add</param>
        /// <param name="value">The value of the variable to add</param>
        public void SetHidden(string name, dynamic value)
        {
            if (All.Any(v => v.Name == name && v.Hidden))
                All.FirstOrDefault(v => v.Name == name && v.Hidden).Value = value;
            else
                All.Add(new CVar(name, value, false, true));
        }

        /// <summary>
        /// Adds a CVar object to the variable list only if no other variable with the same name exists.
        /// </summary>
        /// <param name="variable">The CVar object to add</param>
        public void SetNew(CVar variable)
        {           
            if (!VariableExists(variable.Name)) Set(variable);
        }

        /// <summary>
        /// Removes a non-hidden variable given its name.
        /// </summary>
        /// <param name="name">The name of the variable to remove</param>
        public void Remove(string name)
        {
            All.RemoveAll(v => v.Name == name && !v.Hidden);
        }

        /// <summary>
        /// <para>Removes all non-hidden variables if their name matches a condition.</para>
        /// <para>The name of the variables in the list are the left-hand term of the comparison.</para>
        /// </summary>
        /// <param name="comparer">The comparison operator</param>
        /// <param name="name">The right-hand term of the comparison</param>
        /// <param name="data">The BotData object used for variable replacement</param>
        public void Remove(Comparer comparer, string name, BotData data)
        {
            All.RemoveAll(v => Condition.ReplaceAndVerify(v.Name, comparer, name, data) && !v.Hidden);
        }

        /// <summary>
        /// Generates a string where all the variables marked as Capture are printed in an organized fashion.
        /// </summary>
        /// <returns>The chained capture string.</returns>
        public string ToCaptureString()
        {
            return string.Join(" | ", Captures.Select(c => c.Name + " = " + c.ToString()));
        }
    }
}
