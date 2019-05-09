using RuriLib.LS;
using RuriLib.Models;
using RuriLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Windows.Media;

namespace RuriLib
{
    /// <summary>
    /// The available utility groups.
    /// </summary>
    public enum UtilityGroup
    {
        /// <summary>The group of actions performed on list variables.</summary>
        List,

        /// <summary>The group of actions performed on single variables.</summary>
        Variable,

        /// <summary>The group of action to convert an encoded string.</summary>
        Conversion,

        /// <summary>The group of actions that interact with files.</summary>
        File
    }

    /// <summary>
    /// Actions executed on single variables.
    /// </summary>
    public enum VarAction
    {
        /// <summary>Splits a variable into a list given a separator.</summary>
        Split
    }

    /// <summary>
    /// Actions executed on list variables.
    /// </summary>
    public enum ListAction
    {
        /// <summary>Creates an empty list variable.</summary>
        Create,

        /// <summary>Calculates the length of a list.</summary>
        Length,

        /// <summary>Joins a list into a single string, separating the elements with a separator.</summary>
        Join,

        /// <summary>Sorts a list alphabetically, in ascending or descending order.</summary>
        Sort,

        /// <summary>Concatenates two lists into a longer list variable.</summary>
        Concat,

        /// <summary>Zips two lists into a new list variable where the elements are joined two by two.</summary>
        Zip,

        /// <summary>Maps two lists into a dictionary variable.</summary>
        Map,

        /// <summary>Adds an element to a list variable.</summary>
        Add,

        /// <summary>Removes an element from a list variable.</summary>
        Remove,

        /// <summary>Picks a random element from a list variable.</summary>
        Random
    }

    /// <summary>
    /// The available conversion formats.
    /// </summary>
    public enum Conversion
    {
        /// <summary>A hexadecimal representation of a byte array.</summary>
        HEX,

        /// <summary>A binary representation of a byte array, containing a multiple of 8 binary digits.</summary>
        BIN,

        /// <summary>A base64 representation of a byte array.</summary>
        BASE64
    }

    /// <summary>
    /// Actions that can be performed on files.
    /// </summary>
    public enum FileAction
    {
        /// <summary>Reads a file to a single variable.</summary>
        Read,

        /// <summary>Reads a file to a list variable.</summary>
        ReadLines,

        /// <summary>Writes a single variable to a file.</summary>
        Write,

        /// <summary>Writes a list variable to a file.</summary>
        WriteLines,

        /// <summary>Appends a single variable to a file.</summary>
        Append,

        /// <summary>Appends a list variable to a file.</summary>
        AppendLines
    }

    /// <summary>
    /// A block that performs actions on variables, converts values and operates on files.
    /// </summary>
    public class BlockUtility : BlockBase
    {
        // General
        private UtilityGroup group = UtilityGroup.List;
        /// <summary>The utility group.</summary>
        public UtilityGroup Group { get { return group; } set { group = value; OnPropertyChanged(); } }

        private string variableName = "";
        /// <summary>The name of the output variable.</summary>
        public string VariableName { get { return variableName; } set { variableName = value; OnPropertyChanged(); } }

        private bool isCapture = false;
        /// <summary>Whether the output variable should be marked for Capture.</summary>
        public bool IsCapture { get { return isCapture; } set { isCapture = value; OnPropertyChanged(); } }

        private string inputString = "";
        /// <summary>The input to process.</summary>
        public string InputString { get { return inputString; } set { inputString = value; OnPropertyChanged(); } }

        // Lists
        private ListAction listAction = ListAction.Join;
        /// <summary>The action to be performed on a list variable.</summary>
        public ListAction ListAction { get { return listAction; } set { listAction = value; OnPropertyChanged(); } }

        private string listName = "";
        /// <summary>The name of the target list variable.</summary>
        public string ListName { get { return listName; } set { listName = value; OnPropertyChanged(); } }

        #region ListUtility Specific Variables
        private string separator = ","; // Join
        /// <summary>The separator for joining a list.</summary>
        public string Separator { get { return separator; } set { separator = value; OnPropertyChanged(); } }

        private bool ascending = true;
        /// <summary>Whether the sort should happen in ascending order.</summary>
        public bool Ascending { get { return ascending; } set { ascending = value; OnPropertyChanged(); } }

        private bool numeric = false;
        /// <summary>Whether a list is made of numeric values.</summary>
        public bool Numeric { get { return numeric; } set { numeric = value; OnPropertyChanged(); } }

        private string secondListName = ""; // Zip
        /// <summary>The name of the second list variable.</summary>
        public string SecondListName { get { return secondListName; } set { secondListName = value; OnPropertyChanged(); } }

        private string listItem = ""; // Add
        /// <summary>The list item to add.</summary>
        public string ListItem { get { return listItem; } set { listItem = value; OnPropertyChanged(); } }

        private string listIndex = "-1"; // Add (-1 = end, 0 = start)
        /// <summary>The list index where an item can be added/removed. -1 = end, 0 = start.</summary>
        public string ListIndex { get { return listIndex; } set { listIndex = value; OnPropertyChanged(); } }
        #endregion

        // Variables
        private VarAction varAction = VarAction.Split;
        /// <summary>The action to be performed on a single variable.</summary>
        public VarAction VarAction { get { return varAction; } set { varAction = value; OnPropertyChanged(); } }

        private string varName = "";
        /// <summary>The name of the single variable.</summary>
        public string VarName { get { return varName; } set { varName = value; OnPropertyChanged(); } }

        #region Variables Specific
        private string splitSeparator = "";
        /// <summary>The separator to split a string into a list.</summary>
        public string SplitSeparator { get { return splitSeparator; } set { splitSeparator = value; OnPropertyChanged(); } }
        #endregion

        // Conversion
        private Conversion conversionFrom = Conversion.HEX;
        /// <summary>The encoding to convert from.</summary>
        public Conversion ConversionFrom { get { return conversionFrom; } set { conversionFrom = value; OnPropertyChanged(); } }

        private Conversion conversionTo = Conversion.BASE64;
        /// <summary>The encoding to convert to.</summary>
        public Conversion ConversionTo { get { return conversionTo; } set { conversionTo = value; OnPropertyChanged(); } }

        // Files
        private string filePath = "test.txt";
        /// <summary>The path to the file to read/write.</summary>
        public string FilePath { get { return filePath; } set { filePath = value; OnPropertyChanged(); } }

        private FileAction fileAction = FileAction.Read;
        /// <summary>The action to be performed on the file.</summary>
        public FileAction FileAction { get { return fileAction; } set { fileAction = value; OnPropertyChanged(); } }

        /// <summary>
        /// Creates a Utility block.
        /// </summary>
        public BlockUtility()
        {
            Label = "UTILITY";
        }

        /// <inheritdoc />
        public override BlockBase FromLS(string line)
        {
            // Trim the line
            var input = line.Trim();

            // Parse the label
            if (input.StartsWith("#"))
                Label = LineParser.ParseLabel(ref input);

            // Parse the function
            Group = (UtilityGroup)LineParser.ParseEnum(ref input, "Group", typeof(UtilityGroup));

            // Parse specific function parameters
            switch (Group)
            {
                case UtilityGroup.List:

                    ListName = LineParser.ParseLiteral(ref input, "List Name");
                    ListAction = (ListAction)LineParser.ParseEnum(ref input, "List Action", typeof(ListAction));

                    switch (ListAction)
                    {
                        case ListAction.Join:
                            Separator = LineParser.ParseLiteral(ref input, "Separator");
                            break;

                        case ListAction.Sort:
                            while (LineParser.Lookahead(ref input) == TokenType.Boolean)
                                LineParser.SetBool(ref input, this);
                            break;

                        case ListAction.Concat:
                        case ListAction.Zip:
                        case ListAction.Map:
                            SecondListName = LineParser.ParseLiteral(ref input, "Second List Name");
                            break;

                        case ListAction.Add:
                            ListItem = LineParser.ParseLiteral(ref input, "Item");
                            ListIndex = LineParser.ParseLiteral(ref input, "Index");
                            break;

                        case ListAction.Remove:
                            ListIndex = LineParser.ParseLiteral(ref input, "Index");
                            break;
                    }
                    break;

                case UtilityGroup.Variable:
                    VarName = LineParser.ParseLiteral(ref input, "Var Name");
                    VarAction = (VarAction)LineParser.ParseEnum(ref input, "Var Action", typeof(VarAction));

                    switch (VarAction)
                    {
                        case VarAction.Split:
                            SplitSeparator = LineParser.ParseLiteral(ref input, "Separator");
                            break;
                    }
                    break;

                case UtilityGroup.Conversion:
                    ConversionFrom = (Conversion)LineParser.ParseEnum(ref input, "Conversion From", typeof(Conversion));
                    ConversionTo = (Conversion)LineParser.ParseEnum(ref input, "Conversion To", typeof(Conversion));
                    InputString = LineParser.ParseLiteral(ref input, "Input");
                    break;

                case UtilityGroup.File:
                    FilePath = LineParser.ParseLiteral(ref input, "File Name");
                    FileAction = (FileAction)LineParser.ParseEnum(ref input, "File Action", typeof(FileAction));

                    switch (FileAction)
                    {
                        case FileAction.Write:
                        case FileAction.WriteLines:
                        case FileAction.Append:
                        case FileAction.AppendLines:
                            InputString = LineParser.ParseLiteral(ref input, "Input String");
                            break;
                    }
                    break;
            }

            // Try to parse the arrow, otherwise just return the block as is with default var name and var / cap choice
            if (LineParser.ParseToken(ref input, TokenType.Arrow, false) == "")
                return this;

            // Parse the VAR / CAP
            try
            {
                var varType = LineParser.ParseToken(ref input, TokenType.Parameter, true);
                if (varType.ToUpper() == "VAR" || varType.ToUpper() == "CAP")
                    IsCapture = varType.ToUpper() == "CAP";
            }
            catch { throw new ArgumentException("Invalid or missing variable type"); }

            // Parse the variable/capture name
            try { VariableName = LineParser.ParseToken(ref input, TokenType.Literal, true); }
            catch { throw new ArgumentException("Variable name not specified"); }

            return this;
        }

        /// <inheritdoc />
        public override string ToLS(bool indent = true)
        {
            var writer = new BlockWriter(GetType(), indent, Disabled);
            writer                
                .Label(Label)
                .Token("UTILITY")
                .Token(Group);

            switch (Group)
            {
                case UtilityGroup.List:
                    writer
                        .Literal(ListName)
                        .Token(ListAction);

                    switch (ListAction)
                    {
                        case ListAction.Join:
                            writer
                                .Literal(Separator);
                            break;

                        case ListAction.Sort:
                            writer
                                .Boolean(Ascending, "Ascending")
                                .Boolean(Numeric, "Numeric");
                            break;

                        case ListAction.Concat:
                        case ListAction.Zip:
                        case ListAction.Map:
                            writer
                                .Literal(SecondListName);
                            break;

                        case ListAction.Add:
                            writer
                                .Literal(ListItem)
                                .Literal(ListIndex);
                            break;

                        case ListAction.Remove:
                            writer
                                .Literal(ListIndex);
                            break;
                    }
                    break;

                case UtilityGroup.Variable:
                    writer
                        .Literal(VarName)
                        .Token(VarAction);

                    switch (VarAction)
                    {
                        case VarAction.Split:
                            writer
                                .Literal(SplitSeparator);
                            break;
                    }
                    break;

                case UtilityGroup.Conversion:
                    writer
                        .Token(ConversionFrom)
                        .Token(ConversionTo)
                        .Literal(InputString);
                    break;

                case UtilityGroup.File:
                    writer
                        .Literal(FilePath)
                        .Token(FileAction);

                    switch (FileAction)
                    {
                        case FileAction.Write:
                        case FileAction.WriteLines:
                        case FileAction.Append:
                        case FileAction.AppendLines:
                            writer
                                .Literal(InputString);
                            break;
                    }
                    break;
            }

            if (!writer.CheckDefault(VariableName, "VariableName"))
                writer
                    .Arrow()
                    .Token(IsCapture ? "CAP" : "VAR")
                    .Literal(VariableName);

            return writer.ToString();
        }

        /// <inheritdoc />
        public override void Process(BotData data)
        {
            base.Process(data);

            try
            {
                switch (group)
                {
                    case UtilityGroup.List:

                        var list = data.Variables.GetList(listName);
                        var list2 = data.Variables.GetList(secondListName);
                        var item = ReplaceValues(listItem, data);
                        var index = int.Parse(ReplaceValues(listIndex, data));
                        CVar variable = null;

                        switch (listAction)
                        {
                            case ListAction.Create:
                                data.Variables.Set(new CVar(variableName, new List<string>(), isCapture));
                                break;

                            case ListAction.Length:
                                data.Variables.Set(new CVar(variableName, list.Count().ToString(), isCapture));
                                break;

                            case ListAction.Join:
                                data.Variables.Set(new CVar(variableName, string.Join(separator, list), isCapture));
                                break;

                            case ListAction.Sort:
                                var sorted = list.Select(e => e).ToList(); // Clone the list so we don't edit the original one
                                if (Numeric)
                                {
                                    var nums = sorted.Select(e => double.Parse(e, CultureInfo.InvariantCulture)).ToList();
                                    nums.Sort();
                                    sorted = nums.Select(e => e.ToString()).ToList();
                                }
                                else
                                {
                                    sorted.Sort();
                                }                                
                                if (!Ascending) sorted.Reverse();
                                data.Variables.Set(new CVar(variableName, sorted, isCapture));
                                break;

                            case ListAction.Concat:
                                data.Variables.Set(new CVar(variableName, list.Concat(list2).ToList(), isCapture));
                                break;

                            case ListAction.Zip:
                                data.Variables.Set(new CVar(variableName, list.Zip(list2, (a, b) => a + b).ToList(), isCapture));
                                break;

                            case ListAction.Map:
                                data.Variables.Set(new CVar(variableName, list.Zip(list2, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v), isCapture));
                                break;

                            case ListAction.Add:
                                variable = data.Variables.Get(listName, CVar.VarType.List);
                                if (variable == null) variable = data.GlobalVariables.Get(listName, CVar.VarType.List);
                                if (variable == null) break;
                                if (variable.Value.Count == 0) index = 0;
                                else if (index < 0) index += variable.Value.Count;
                                variable.Value.Insert(index, item);
                                break;

                            case ListAction.Remove:
                                variable = data.Variables.Get(listName, CVar.VarType.List);
                                if (variable == null) variable = data.GlobalVariables.Get(listName, CVar.VarType.List);
                                if (variable == null) break;
                                if (variable.Value.Count == 0) index = 0;
                                else if (index < 0) index += variable.Value.Count;
                                variable.Value.RemoveAt(index);
                                break;

                            case ListAction.Random:
                                data.Variables.Set(new CVar(variableName, list[data.rand.Next(list.Count)], isCapture));
                                break;

                            default:
                                break;
                        }

                        data.Log(new LogEntry(string.Format("Executed action {0} on list {1}", listAction, listName), Colors.White));
                        break;

                    case UtilityGroup.Variable:
                        string single;
                        switch (varAction)
                        {
                            case VarAction.Split:
                                single = data.Variables.GetSingle(varName);
                                data.Variables.Set(new CVar(variableName, single.Split(new string[] { ReplaceValues(separator, data) }, StringSplitOptions.None).ToList(), isCapture));
                                break;
                        }
                        data.Log(new LogEntry(string.Format("Executed action {0} on variable {1}", varAction, varName), Colors.White));
                        break;

                    case UtilityGroup.Conversion:
                        byte[] convertedBytes = ConvertFrom(ReplaceValues(inputString, data), conversionFrom);
                        data.Variables.Set(new CVar(variableName, ConvertTo(convertedBytes, conversionTo), isCapture));
                        data.Log(new LogEntry(string.Format("Converted input from {0} to {1}", conversionFrom, conversionTo), Colors.White));
                        break;

                    case UtilityGroup.File:
                        var file = ReplaceValues(filePath, data);
                        var input = ReplaceValues(inputString, data).Replace("\\r\\n", "\r\n").Replace("\\n", "\n");
                        var inputs = ReplaceValuesRecursive(inputString, data);
                        switch (fileAction)
                        {
                            case FileAction.Read:
                                data.Variables.Set(new CVar(variableName, File.ReadAllText(file), isCapture));
                                break;

                            case FileAction.ReadLines:
                                data.Variables.Set(new CVar(variableName, File.ReadAllLines(file).ToList(), isCapture));
                                break;

                            case FileAction.Write:
                                File.WriteAllText(file, input);
                                break;

                            case FileAction.WriteLines:
                                File.WriteAllLines(file, inputs);
                                break;

                            case FileAction.Append:
                                File.AppendAllText(file, input);
                                break;

                            case FileAction.AppendLines:
                                File.AppendAllLines(file, inputs);
                                break;
                        }
                        break;

                    default:
                        break;
                }
            }
            catch(Exception ex) { data.Log(new LogEntry(ex.Message, Colors.Tomato)); }
        }

        /// <summary>
        /// Converts an encoded input to a byte array.
        /// </summary>
        /// <param name="input">The encoded input</param>
        /// <param name="type">The encoding</param>
        /// <returns>The converted byte array</returns>
        public byte[] ConvertFrom(string input, Conversion type)
        {
            switch (type)
            {
                case Conversion.BASE64:
                    return Convert.FromBase64String(input);

                case Conversion.HEX:
                    input = new string(input.ToCharArray()
                                .Where(c => !char.IsWhiteSpace(c))
                                .ToArray()).Replace("0x", "");
                    return Enumerable.Range(0, input.Length)
                     .Where(x => x % 2 == 0)
                     .Select(x => Convert.ToByte(input.Substring(x, 2), 16))
                     .ToArray();

                case Conversion.BIN:
                    int numOfBytes = input.Length / 8;
                    byte[] bytes = new byte[numOfBytes];
                    for (int i = 0; i < numOfBytes; ++i) { bytes[i] = Convert.ToByte(input.Substring(8 * i, 8), 2); }
                    return bytes;

                default:
                    return new byte[0];
            }
        }

        /// <summary>
        /// Converts a byte array to an encoded string.
        /// </summary>
        /// <param name="input">The byte array to encode</param>
        /// <param name="type">The encoding</param>
        /// <returns>The encoded string</returns>
        public string ConvertTo(byte[] input, Conversion type)
        {
            StringBuilder sb = new StringBuilder();
            switch (type)
            {
                case Conversion.BASE64:
                    return Convert.ToBase64String(input);

                case Conversion.HEX:
                    foreach (byte b in input)
                        sb.AppendFormat("{0:x2}", b);
                    return sb.ToString().ToUpper();

                case Conversion.BIN:
                    return string.Concat(input.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));

                default:
                    return "";
            }
        }
    }
}
