using RuriLib.ViewModels;

namespace RuriLib.Models
{
    /// <summary>
    /// Represents a Key in a KeyChain.
    /// </summary>
    public class Key : ViewModelBase
    {
        private string leftTerm = "<SOURCE>";
        /// <summary>The left-hand term for the comparison.</summary>
        public string LeftTerm { get { return leftTerm; } set { leftTerm = value; OnPropertyChanged(); } }

        private Condition condition = Condition.Contains;
        /// <summary>The condition of the comparison.</summary>
        public Condition Condition { get { return condition; } set { condition = value; OnPropertyChanged(); } }

        private string rightTerm = "";
        /// <summary>The right-hand term of the comparison.</summary>
        public string RightTerm { get { return rightTerm; } set { rightTerm = value; OnPropertyChanged(); } }
        
        /// <summary>
        /// Checks the comparison between left and right member.
        /// </summary>
        /// <param name="data">The BotData used for variable replacement.</param>
        /// <returns>Whether the comparison is valid</returns>
        public bool CheckKey(BotData data)
        {
            try
            {
                return ConditionChecker.Verify(LeftTerm, Condition, RightTerm, data);
            }
            catch { return false; } // Return false if e.g. we can't parse the number for a LessThan/GreaterThan comparison.
        }
    }
}
