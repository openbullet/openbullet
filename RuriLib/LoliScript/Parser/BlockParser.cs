﻿using System;
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
        public static Dictionary<string, Type> BlockMappings { get; set; } = new Dictionary<string, Type>()
        {
            { "BYPASSCF", typeof(BlockBypassCF) },
            { "CAPTCHA", typeof(BlockImageCaptcha) },
            { "FUNCTION", typeof(BlockFunction) },
            { "KEYCHECK", typeof(BlockKeycheck) },
            { "PARSE", typeof(BlockParse) },
            { "RECAPTCHA", typeof(BlockRecaptcha) },
            { "REQUEST", typeof(BlockRequest) },
            { "TCP", typeof(BlockTCP) },
            { "UTILITY", typeof(BlockUtility) },
            { "BROWSERACTION", typeof(SBlockBrowserAction) },
            { "ELEMENTACTION", typeof(SBlockElementAction) },
            { "EXECUTEJS", typeof(SBlockExecuteJS) },
            { "NAVIGATE", typeof(SBlockNavigate) }
        };

        /// <summary>
        /// Tests if a line is parsable as a block.
        /// </summary>
        /// <param name="line">The data line to test</param>
        /// <returns>Whether the line contains a block or not.</returns>
        public static bool IsBlock(string line)
        {
            return BlockMappings
                .Keys
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
            if (input == string.Empty) throw new ArgumentNullException();

            // Parse if disabled or not
            var disabled = input.StartsWith("!");
            if (disabled) input = input.Substring(1).Trim();

            var label = LineParser.ParseToken(ref input, TokenType.Label, false);

            // Parse the identifier
            var identifier = "";
            try { identifier = LineParser.ParseToken(ref input, TokenType.Parameter, true); }
            catch { throw new ArgumentException("Missing identifier"); }

            // Create the actual block from the identifier
            BlockBase block = (Activator.CreateInstance(BlockMappings[identifier]) as BlockBase).FromLS(input);
            
            // Set disabled
            if (block != null) block.Disabled = disabled;

            // Set the label
            if (block != null && label != string.Empty) block.Label = label.Replace("#", "");

            return block;
        }
    }
}
