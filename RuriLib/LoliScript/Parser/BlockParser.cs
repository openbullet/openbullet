using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RuriLib.LS
{
    /// <summary>
    /// Parses a block from LoliScript code.
    /// </summary>
    public static class BlockParser
    {
        /// <summary>
        /// The allowed block identifiers.
        /// </summary>
        public enum BlockName
        {
            /// <summary>The BYPASSCF block.</summary>
            BYPASSCF,

            /// <summary>The CAPTCHA block.</summary>
            CAPTCHA,

            /// <summary>The FUNCTION block.</summary>
            FUNCTION,

            /// <summary>The KEYCHECK block.</summary>
            KEYCHECK,

            /// <summary>The PARSE block.</summary>
            PARSE,

            /// <summary>The RECAPTCHA block.</summary>
            RECAPTCHA,

            /// <summary>The REQUEST block.</summary>
            REQUEST,

            /// <summary>The TCP block.</summary>
            TCP,

            /// <summary>The UTILITY block.</summary>
            UTILITY,

            /// <summary>The BROWSERACTION block.</summary>
            BROWSERACTION,

            /// <summary>The ELEMENTACTION block.</summary>
            ELEMENTACTION,

            /// <summary>The EXECUTEJS block.</summary>
            EXECUTEJS,

            /// <summary>The NAVIGATE block.</summary>
            NAVIGATE
        }

        /// <summary>
        /// Tests if a line is parsable as a block.
        /// </summary>
        /// <param name="line">The data line to test</param>
        /// <returns>Whether the line contains a block or not.</returns>
        public static bool IsBlock(string line)
        {
            return Enum.GetNames(typeof(BlockName))
                .Select(n => n.ToUpper())
                .Contains(GetBlockType(line).ToUpper());
        }

        /// <summary>
        /// Gets the block type from a block line.
        /// </summary>
        /// <param name="line">The block line</param>
        /// <returns>The type of the block</returns>
        public static string GetBlockType(string line)
        {
            var groups = Regex.Match(line, @"^!?(#[^ ]* )?([^ ]*)").Groups;
            return groups[2].Value;
        }

        /// <summary>
        /// Parses a block line as a block object.
        /// </summary>
        /// <param name="line">The block line</param>
        /// <returns>The parsed block object</returns>
        public static BlockBase Parse(string line)
        {
            // Trim the line
            var input = line.Trim();

            // Return an exception if the line is empty
            if (input == "") throw new ArgumentNullException();

            // Parse if disabled or not
            var disabled = input.StartsWith("!");
            if (disabled) input = input.Substring(1).Trim();

            var label = LineParser.ParseToken(ref input, TokenType.Label, false);

            // Parse the identifier
            var identifier = "";
            try { identifier = LineParser.ParseToken(ref input, TokenType.Parameter, true); }
            catch { throw new ArgumentException("Missing identifier"); }

            BlockBase block = null;
            switch ((BlockName)Enum.Parse(typeof(BlockName), identifier, true))
            {
                case BlockName.FUNCTION:
                    block = (new BlockFunction()).FromLS(input);
                    break;

                case BlockName.KEYCHECK:
                    block = (new BlockKeycheck()).FromLS(input);
                    break;

                case BlockName.RECAPTCHA:
                    block = (new BlockRecaptcha()).FromLS(input);
                    break;

                case BlockName.REQUEST:
                    block = (new BlockRequest()).FromLS(input);
                    break;

                case BlockName.PARSE:
                    block = (new BlockParse()).FromLS(input);
                    break;

                case BlockName.CAPTCHA:
                    block = (new BlockImageCaptcha()).FromLS(input);
                    break;

                case BlockName.BYPASSCF:
                    block = (new BlockBypassCF()).FromLS(input);
                    break;

                case BlockName.UTILITY:
                    block = (new BlockUtility()).FromLS(input);
                    break;

                case BlockName.TCP:
                    block = (new BlockTCP()).FromLS(input);
                    break;

                case BlockName.NAVIGATE:
                    block = (new SBlockNavigate()).FromLS(input);
                    break;

                case BlockName.BROWSERACTION:
                    block = (new SBlockBrowserAction()).FromLS(input);
                    break;

                case BlockName.ELEMENTACTION:
                    block = (new SBlockElementAction()).FromLS(input);
                    break;

                case BlockName.EXECUTEJS:
                    block = (new SBlockExecuteJS()).FromLS(input);
                    break;
                    
                default:
                    throw new ArgumentException($"Invalid identifier '{identifier}'");
            }

            // Set disabled
            if (block != null) block.Disabled = disabled;

            // Set the label
            if (block != null && label != "") block.Label = label.Replace("#", "");

            return block;
        }
    }
}
