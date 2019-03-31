using System.Windows.Media;

namespace RuriLib.Models
{
    /// <summary>
    /// A KeyChain that can be customized in name and color.
    /// </summary>
    public class CustomKeychain
    {
        /// <summary>The name of the KeyChain.</summary>
        public string Name { get; set; } = "CUSTOM";

        /// <summary>The color of the KeyChain.</summary>
        public Color Color { get; set; } = Colors.OrangeRed;
    }
}
