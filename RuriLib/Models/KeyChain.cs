using RuriLib.Models;
using RuriLib.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace RuriLib.Models
{
    /// <summary>
    /// Represents a set of keys that can be checked in a OR/AND fashion and modifies the status of the BotData if successful.
    /// </summary>
    public class KeyChain : ViewModelBase
    {
        /// <summary>The returned status upon a successful check of the keys. If no KeyChain was valid, the original status won't be changed.</summary>        
        public enum KeychainType
        {
            /// <summary>Sets a SUCCESS status in the bot.</summary>
            Success,

            /// <summary>Sets a FAIL status in the bot.</summary>
            Failure,

            /// <summary>Sets a BAN status in the bot.</summary>
            Ban,

            /// <summary>Sets a RETRY status in the bot.</summary>
            Retry,

            /// <summary>Sets a CUSTOM status in the bot.</summary>
            Custom
        }
        
        /// <summary>The mode in which the keys should be checked.</summary>
        public enum KeychainMode
        {
            /// <summary>Trigger the KeyChain if ANY Key is valid.</summary>
            OR,

            /// <summary>Trigger the KeyChain if ALL the Keys are valid.</summary>
            AND
        }

        private KeychainType type = KeychainType.Success;
        /// <summary>The type of the KeyChain.</summary>
        public KeychainType Type { get { return type; } set { type = value; OnPropertyChanged(); } }

        private KeychainMode mode = KeychainMode.OR;
        /// <summary>The mode of the KeyChain.</summary>
        public KeychainMode Mode { get { return mode; } set { mode = value; OnPropertyChanged(); } }

        private string customType = "CUSTOM";
        /// <summary>The type of the KeyChain in case Custom is selected.</summary>
        public string CustomType { get { return customType; } set { customType = value; OnPropertyChanged(); } }

        /// <summary>The collection of Keys in the KeyChain.</summary>
        public ObservableCollection<Key> Keys { get; set; } = new ObservableCollection<Key>();
        
        /// <summary>
        /// Checks all the Keys in the KeyChain.
        /// </summary>
        /// <param name="data">The BotData used for variable replacement</param>
        /// <returns>Whether the KeyChain was triggered or not</returns>
        public bool CheckKeys(BotData data)
        {
            switch (Mode)
            {
                case KeychainMode.OR:
                    foreach (var key in Keys)
                    {
                        if (key.CheckKey(data))
                        {
                            data.Log(new LogEntry(string.Format("Found 'OR' Key {0} {1} {2}", 
                                BlockBase.TruncatePretty(BlockBase.ReplaceValues(key.LeftTerm, data), 20),
                                key.Comparer.ToString(),
                                BlockBase.ReplaceValues(key.RightTerm, data)),
                                Colors.White));
                            return true;
                        }
                    }
                    return false;

                case KeychainMode.AND:
                    foreach (var key in Keys)
                    {
                        if (!key.CheckKey(data))
                            return false;
                        else
                        {
                            data.Log(new LogEntry(string.Format("Found 'AND' Key {0} {1} {2}", 
                                BlockBase.TruncatePretty(BlockBase.ReplaceValues(key.LeftTerm, data), 20),
                                key.Comparer.ToString(),
                                BlockBase.ReplaceValues(key.RightTerm, data)),
                                Colors.White));
                        }
                    }
                    return true;
            }
            return false;
        }
    }
}
