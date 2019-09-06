using Extreme.Net;
using RuriLib.LS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Media;

namespace RuriLib
{
    /// <summary>
    /// A block that can execute a specific function on one or multiple inputs.
    /// </summary>
    public class BlockFunction : BlockBase
    {
        /// <summary>
        /// The function name.
        /// </summary>
        public enum Function
        {
            /// <summary>Simply replaced the variables of the input.</summary>
            Constant,

            /// <summary>Encodes an input as a base64 string.</summary>
            Base64Encode,

            /// <summary>Decodes the string from a base64-encoded input.</summary>
            Base64Decode,

            /// <summary>Hashes an input string.</summary>
            Hash,

            /// <summary>Generates a HMAC for a given string.</summary>
            HMAC,

            /// <summary>Translates words in a given string.</summary>
            Translate,

            /// <summary>Converts a formatted date to a unix timestamp.</summary>
            DateToUnixTime,

            /// <summary>Gets the length of a string.</summary>
            Length,

            /// <summary>Converts all uppercase caracters in a string to lowercase.</summary>
            ToLowercase,

            /// <summary>Converts all lowercase characters in a string to uppercase.</summary>
            ToUppercase,

            /// <summary>Replaces some text with something else, with or without using regex.</summary>
            Replace,

            /// <summary>Gets the first match for a specific regex pattern.</summary>
            RegexMatch,

            /// <summary>Encodes the input to be used in a URL.</summary>
            URLEncode,

            /// <summary>Decodes a URL-encoded input.</summary>
            URLDecode,
            
            /// <summary>Unescapes characters in a string.</summary>
            Unescape,      

            /// <summary>Encodes the input to be displayed in HTML or XML.</summary>
            HTMLEntityEncode,

            /// <summary>Decoded an input containing HTML or XML entities.</summary>
            HTMLEntityDecode,

            /// <summary>Converts a unix timestamp to a formatted date.</summary>
            UnixTimeToDate,

            /// <summary>Retrieves the current time as a unix timestamp.</summary>
            CurrentUnixTime,

            /// <summary>Converts a unix timestamp to the ISO8601 format.</summary>
            UnixTimeToISO8601,

            /// <summary>Generates a random integer.</summary>
            RandomNum,

            /// <summary>Generates a random string based on a mask.</summary>
            RandomString,

            /// <summary>Rounds a decimal input to the upper integer.</summary>
            Ceil,

            /// <summary>Rounds a decimal input to the lower integer.</summary>
            Floor,

            /// <summary>Rounds a decimal input to the nearest integer.</summary>
            Round,

            /// <summary>Computes mathematical operations between decimal numbers.</summary>
            Compute,

            /// <summary>Counts the occurrences of a string in another string.</summary>
            CountOccurrences,

            /// <summary>Clears the cookie jar used for HTTP requests.</summary>
            ClearCookies,

            /// <summary>Encrypts a string with RSA given a Key.</summary>
            RSA,

            /// <summary>Waits a given amount of milliseconds.</summary>
            Delay,

            /// <summary>Retrieves the character at a given index in the input string.</summary>
            CharAt,

            /// <summary>Gets a substring of the input.</summary>
            Substring,

            /// <summary>Reverses the input string.</summary>
            ReverseString,

            /// <summary>Removes leading or trailing whitespaces from a string.</summary>
            Trim,

            /// <summary>Gets a valid random User-Agent header.</summary>
            GetRandomUA,

            /// <summary>Encrypts a string with AES.</summary>
            AESEncrypt,

            /// <summary>Decrypts an AES-encrypted string.</summary>
            AESDecrypt
        }

        /// <summary>
        /// The available hashing functions.
        /// </summary>
        public enum Hash
        {
            /// <summary>The MD5 hashing function (128 bits digest).</summary>
            MD5,

            /// <summary>The SHA-1 hashing function (160 bits digest).</summary>
            SHA1,

            /// <summary>The SHA-256 hashing function (256 bits digest).</summary>
            SHA256,

            /// <summary>The SHA-384 hashing function (384 bits digest).</summary>
            SHA384,

            /// <summary>The SHA-512 hashing function (512 bits digest).</summary>
            SHA512,
        }

        #region General Properties
        private string variableName = "";
        /// <summary>The name of the output variable.</summary>
        public string VariableName { get { return variableName; } set { variableName = value; OnPropertyChanged(); } }

        private bool isCapture = false;
        /// <summary>Whether the output variable should be marked for Capture.</summary>
        public bool IsCapture { get { return isCapture; } set { isCapture = value; OnPropertyChanged(); } }

        private string inputString = "";
        /// <summary>The input string on which the function will be executed (not always needed).</summary>
        public string InputString { get { return inputString; } set { inputString = value; OnPropertyChanged(); } }

        private Function functionType = Function.Constant;
        /// <summary>The function to execute.</summary>
        public Function FunctionType { get { return functionType; } set { functionType = value; OnPropertyChanged(); } }
        #endregion

        #region Function Specific Properties
        // -- Hash & Hmac
        private Hash hashType = Hash.SHA512;
        /// <summary>The hashing function to use.</summary>
        public Hash HashType { get { return hashType; } set { hashType = value; OnPropertyChanged(); } }

        // -- Hmac
        private string hmacKey = "";
        /// <summary>The key used to authenticate the message.</summary>
        public string HmacKey { get { return hmacKey; } set { hmacKey = value; OnPropertyChanged(); } }

        private bool hmacBase64 = false;
        /// <summary>Whether to output the message as a base64-encoded string instead of a hex-encoded string.</summary>
        public bool HmacBase64 { get { return hmacBase64; } set { hmacBase64 = value; OnPropertyChanged(); } }

        // -- Translate
        private bool stopAfterFirstMatch = true;
        /// <summary>Whether to stop translating after the first match.</summary>
        public bool StopAfterFirstMatch { get { return stopAfterFirstMatch; } set { stopAfterFirstMatch = value; OnPropertyChanged(); } }

        /// <summary>The dictionary containing the words and their translation.</summary>
        public Dictionary<string, string> TranslationDictionary { get; set; } = new Dictionary<string, string>();

        // -- Date to unix
        private string dateFormat = "yyyy-MM-dd:HH-mm-ss";
        /// <summary>The format of the date (y = year, M = month, d = day, H = hour, m = minute, s = second).</summary>
        public string DateFormat { get { return dateFormat; } set { dateFormat = value; OnPropertyChanged(); } }

        // -- string replace
        private string replaceWhat = "";
        /// <summary>The text to replace.</summary>
        public string ReplaceWhat { get { return replaceWhat; } set { replaceWhat = value; OnPropertyChanged(); } }

        private string replaceWith = "";
        /// <summary>The replacement text.</summary>
        public string ReplaceWith { get { return replaceWith; } set { replaceWith = value; OnPropertyChanged(); } }

        private bool useRegex = false;
        /// <summary>Whether to use regex for replacing.</summary>
        public bool UseRegex { get { return useRegex; } set { useRegex = value; OnPropertyChanged(); } }

        // -- Regex Match
        private string regexMatch = "";
        /// <summary>The regex pattern to match.</summary>
        public string RegexMatch { get { return regexMatch; } set { regexMatch = value; OnPropertyChanged(); } }

        // -- Random Number
        private int randomMin = 0;
        /// <summary>The minimum random number that can be generated.</summary>
        public int RandomMin { get { return randomMin; } set { randomMin = value; OnPropertyChanged(); } }

        private int randomMax = 0;
        /// <summary>The maximum random number that can be generated.</summary>
        public int RandomMax { get { return randomMax; } set { randomMax = value; OnPropertyChanged(); } }

        // -- CountOccurrences
        private string stringToFind = "";
        /// <summary>The string to count the occurrences of.</summary>
        public string StringToFind { get { return stringToFind; } set { stringToFind = value; OnPropertyChanged(); } }

        // -- RSA
        private string rsaMod = "";
        /// <summary>The modulus of the RSA key.</summary>
        public string RSAMod { get { return rsaMod; } set { rsaMod = value; OnPropertyChanged(); } }

        private string rsaExp = "";
        /// <summary>The exponent of the RSA key.</summary>
        public string RSAExp { get { return rsaExp; } set { rsaExp = value; OnPropertyChanged(); } }

        // --- CharAt
        private string charIndex = "0";
        /// <summary>The index of the wanted character.</summary>
        public string CharIndex { get { return charIndex; } set { charIndex = value; OnPropertyChanged(); } }

        // -- Substring
        private string substringIndex = "0";
        /// <summary>The starting index for the substring.</summary>
        public string SubstringIndex { get { return substringIndex; } set { substringIndex = value; OnPropertyChanged(); } }

        private string substringLength = "1";
        /// <summary>The length of the wanted substring.</summary>
        public string SubstringLength { get { return substringLength; } set { substringLength = value; OnPropertyChanged(); } }

        // -- AES
        private string aesKey = "";
        /// <summary>The keys used for AES encryption and decryption.</summary>
        public string AesKey { get { return aesKey; } set { aesKey = value; OnPropertyChanged(); } }
        #endregion

        /// <summary>
        /// Creates a Function block.
        /// </summary>
        public BlockFunction()
        {
            Label = "FUNCTION";
        }

        /// <inheritdoc />
        public override BlockBase FromLS(string line)
        {
            // Trim the line
            var input = line.Trim();

            // Parse the label
            if (input.StartsWith("#"))
                Label = LineParser.ParseLabel(ref input);

            /*
             * Syntax:
             * FUNCTION Name [ARGUMENTS] ["INPUT STRING"] [-> VAR/CAP "NAME"]
             * */

            // Parse the function
            FunctionType = (Function)LineParser.ParseEnum(ref input, "Function Name", typeof(Function));

            // Parse specific function parameters
            switch (FunctionType)
            {
                case Function.Hash:
                    HashType = LineParser.ParseEnum(ref input, "Hash Type", typeof(Hash));
                    break;

                case Function.HMAC:
                    HashType = LineParser.ParseEnum(ref input, "Hash Type", typeof(Hash));
                    HmacKey = LineParser.ParseLiteral(ref input, "HMAC Key");
                    while (LineParser.Lookahead(ref input) == TokenType.Boolean)
                        LineParser.SetBool(ref input, this);
                    break;

                case Function.Translate:
                    if (LineParser.Lookahead(ref input) == TokenType.Boolean)
                        LineParser.SetBool(ref input, this);
                    TranslationDictionary = new Dictionary<string, string>();
                    while (input != "" && LineParser.Lookahead(ref input) == TokenType.Parameter)
                    {
                        LineParser.EnsureIdentifier(ref input, "KEY");
                        var k = LineParser.ParseLiteral(ref input, "Key");
                        LineParser.EnsureIdentifier(ref input, "VALUE");
                        var v = LineParser.ParseLiteral(ref input, "Value");
                        TranslationDictionary[k] = v;
                    }
                    break;

                case Function.DateToUnixTime:
                    DateFormat = LineParser.ParseLiteral(ref input, "DATE FORMAT");
                    break;

                case Function.Replace:
                    ReplaceWhat = LineParser.ParseLiteral(ref input, "What");
                    ReplaceWith = LineParser.ParseLiteral(ref input, "With");
                    if (LineParser.Lookahead(ref input) == TokenType.Boolean)
                        LineParser.SetBool(ref input, this);
                    break;

                case Function.RegexMatch:
                    RegexMatch = LineParser.ParseLiteral(ref input, "Pattern");
                    break;

                case Function.RandomNum:
                    RandomMin = LineParser.ParseInt(ref input, "Minimum");
                    RandomMax = LineParser.ParseInt(ref input, "Maximum");
                    break;

                case Function.CountOccurrences:
                    StringToFind = LineParser.ParseLiteral(ref input, "string to find");
                    break;

                case Function.CharAt:
                    CharIndex = LineParser.ParseLiteral(ref input, "Index");
                    break;

                case Function.Substring:
                    SubstringIndex = LineParser.ParseLiteral(ref input, "Index");
                    SubstringLength = LineParser.ParseLiteral(ref input, "Length");
                    break;

                case Function.RSA:
                    RSAMod = LineParser.ParseLiteral(ref input, "Modulus");
                    RSAExp = LineParser.ParseLiteral(ref input, "Exponent");
                    break;

                case Function.AESDecrypt:
                case Function.AESEncrypt:
                    AesKey = LineParser.ParseLiteral(ref input, "AES Key");
                    break;

                default:
                    break;
            }

            // Try to parse the input string
            if (LineParser.Lookahead(ref input) == TokenType.Literal)
                InputString = LineParser.ParseLiteral(ref input, "INPUT");

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
                .Token("FUNCTION")
                .Token(FunctionType);

            switch (FunctionType)
            {
                case Function.Hash:
                    writer
                        .Token(HashType);
                    break;

                case Function.HMAC:
                    writer
                        .Token(HashType)
                        .Literal(HmacKey)
                        .Boolean(HmacBase64, "HmacBase64");
                    break;

                case Function.Translate:
                    writer
                        .Boolean(StopAfterFirstMatch, "StopAfterFirstMatch");
                    foreach (var t in TranslationDictionary)
                        writer
                            .Indent()
                            .Token("KEY")
                            .Literal(t.Key)
                            .Token("VALUE")
                            .Literal(t.Value);

                    writer
                        .Indent();
                    break;

                case Function.DateToUnixTime:
                    writer
                        .Literal(DateFormat, "DateFormat");
                    break;

                case Function.Replace:
                    writer
                        .Literal(ReplaceWhat)
                        .Literal(ReplaceWith)
                        .Boolean(UseRegex, "UseRegex");
                    break;

                case Function.RegexMatch:
                    writer
                        .Literal(RegexMatch, "RegexMatch");
                    break;

                case Function.RandomNum:
                    writer
                        .Integer(RandomMin)
                        .Integer(RandomMax);
                    break;

                case Function.CountOccurrences:
                    writer
                        .Literal(StringToFind);
                    break;

                case Function.CharAt:
                    writer
                        .Literal(CharIndex);
                    break;

                case Function.Substring:
                    writer
                        .Literal(SubstringIndex)
                        .Literal(SubstringLength);
                    break;

                case Function.RSA:
                    writer
                        .Literal(RSAMod)
                        .Literal(RSAExp);
                    break;

                case Function.AESDecrypt:
                case Function.AESEncrypt:
                    writer
                        .Literal(AesKey);
                    break;
            }

            writer
                .Literal(InputString, "InputString");
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

            var style = NumberStyles.Number | NumberStyles.AllowCurrencySymbol;
            var provider = new CultureInfo("en-US");

            var localInputStrings = ReplaceValuesRecursive(inputString, data);
            var outputs = new List<string>();

            for(int i = 0; i < localInputStrings.Count; i++)
            {
                var localInputString = localInputStrings[i];
                var outputString = "";

                switch (FunctionType)
                {
                    case Function.Constant:
                        outputString = localInputString;
                        break;

                    case Function.Base64Encode:
                        outputString = Base64Encode(localInputString);
                        break;

                    case Function.Base64Decode:
                        outputString = Base64Decode(localInputString);
                        break;

                    case Function.HTMLEntityEncode:
                        outputString = WebUtility.HtmlEncode(localInputString);
                        break;

                    case Function.HTMLEntityDecode:
                        outputString = WebUtility.HtmlDecode(localInputString);
                        break;

                    case Function.Hash:
                        outputString = GetHash(localInputString, hashType).ToLower();
                        break;

                    case Function.HMAC:
                        outputString = Hmac(localInputString, hashType, ReplaceValues(hmacKey, data), hmacBase64);
                        break;

                    case Function.Translate:
                        outputString = localInputString;
                        foreach (var entry in TranslationDictionary.OrderBy(e => e.Key.Length).Reverse())
                        {
                            if (outputString.Contains(entry.Key))
                            {
                                outputString = outputString.Replace(entry.Key, entry.Value);
                                if (StopAfterFirstMatch) break;
                            }
                        }
                        break;

                    case Function.DateToUnixTime:
                        outputString = (DateTime.ParseExact(localInputString, dateFormat, new CultureInfo("en-US"), DateTimeStyles.AllowWhiteSpaces)
                            .Subtract(new DateTime(1970, 1, 1)))
                            .TotalSeconds
                            .ToString();
                        break;

                    case Function.Length:
                        outputString = localInputString.Length.ToString();
                        break;

                    case Function.ToLowercase:
                        outputString = localInputString.ToLower();
                        break;

                    case Function.ToUppercase:
                        outputString = localInputString.ToUpper();
                        break;

                    case Function.Replace:
                        if (useRegex)
                            outputString = Regex.Replace(localInputString, ReplaceValues(replaceWhat, data), ReplaceValues(replaceWith, data));
                        else
                            outputString = localInputString.Replace(ReplaceValues(replaceWhat, data), ReplaceValues(replaceWith, data));
                        break;

                    case Function.RegexMatch:
                        outputString = Regex.Match(localInputString, ReplaceValues(regexMatch, data)).Value;
                        break;
                        
                    case Function.Unescape:
                        outputString = Regex.Unescape(localInputString);
                        break;                        
                   
                    case Function.URLEncode:
                        outputString = System.Uri.EscapeDataString(localInputString);
                        break;

                    case Function.URLDecode:
                        outputString = System.Uri.UnescapeDataString(localInputString);
                        break;

                    case Function.UnixTimeToDate:
                        try
                        {
                            var loc = localInputString;
                            if (localInputString.Length > 10) loc = localInputString.Substring(0, 10);
                            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                            dtDateTime = dtDateTime.AddSeconds(int.Parse(loc)).ToLocalTime();
                            outputString = dtDateTime.ToShortDateString();
                        }
                        catch { }

                        break;

                    case Function.CurrentUnixTime:
                        outputString = Math.Round((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds).ToString();
                        break;

                    case Function.UnixTimeToISO8601:
                        var loc2 = localInputString;
                        if (localInputString.Length > 10) loc2 = localInputString.Substring(0, 10);
                        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                        dateTime = dateTime.AddSeconds(int.Parse(loc2)).ToLocalTime();
                        outputString = dateTime.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffZ");
                        break;

                    case Function.RandomNum:
                        outputString = (data.Random.Next(randomMin, randomMax)).ToString();
                        break;

                    case Function.RandomString:
                        var reserved = new string[] { "?l", "?u", "?d", "?s", "?h", "?a", "?m", "?i" };
                        var lowercase = "abcdefghijklmnopqrstuvwxyz";
                        var uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                        var digits = "0123456789";
                        var symbols = "\\!\"£$%&/()=?^'{}[]@#,;.:-_*+";
                        var hex = digits + "abcdef";
                        var allchars = lowercase + uppercase + digits + symbols;
                        var udchars = uppercase + digits;
                        var ludchars = lowercase + uppercase + digits;

                        outputString = localInputString;
                        while (reserved.Any(r => outputString.Contains(r))){
                            if (outputString.Contains("?l"))
                                outputString = ReplaceFirst(outputString, "?l", lowercase[data.Random.Next(0, lowercase.Length)].ToString());
                            else if (outputString.Contains("?u"))
                                outputString = ReplaceFirst(outputString, "?u", uppercase[data.Random.Next(0, uppercase.Length)].ToString());
                            else if (outputString.Contains("?d"))
                                outputString = ReplaceFirst(outputString, "?d", digits[data.Random.Next(0, digits.Length)].ToString());
                            else if (outputString.Contains("?s"))
                                outputString = ReplaceFirst(outputString, "?s", symbols[data.Random.Next(0, symbols.Length)].ToString());
                            else if (outputString.Contains("?h"))
                                outputString = ReplaceFirst(outputString, "?h", hex[data.Random.Next(0, hex.Length)].ToString());
                            else if (outputString.Contains("?a"))
                                outputString = ReplaceFirst(outputString, "?a", allchars[data.Random.Next(0, allchars.Length)].ToString());
                            else if (outputString.Contains("?m"))
                                outputString = ReplaceFirst(outputString, "?m", udchars[data.Random.Next(0, udchars.Length)].ToString());
                            else if (outputString.Contains("?i"))
                                outputString = ReplaceFirst(outputString, "?i", ludchars[data.Random.Next(0, ludchars.Length)].ToString());
                         
                        }
                        break;

                    case Function.Ceil:
                        outputString = Math.Ceiling(Decimal.Parse(localInputString, style, provider)).ToString();
                        break;

                    case Function.Floor:
                        outputString = Math.Floor(Decimal.Parse(localInputString, style, provider)).ToString();
                        break;

                    case Function.Round:
                        outputString = Math.Round(Decimal.Parse(localInputString, style, provider), 0, MidpointRounding.AwayFromZero).ToString();
                        break;

                    case Function.Compute:
                        outputString = new DataTable().Compute(localInputString.Replace(',', '.'), null).ToString();
                        break;

                    case Function.CountOccurrences:
                        outputString = CountStringOccurrences(localInputString, stringToFind).ToString();
                        break;

                    case Function.ClearCookies:
                        data.Cookies.Clear();
                        break;

                    case Function.RSA:
                        try
                        {
                            while (outputString.Length < 2 || outputString.Substring(outputString.Length - 2) != "==")
                            {
                                outputString = SteamRSAEncrypt(new RsaParameters { Exponent = ReplaceValues(RSAExp, data), Modulus = ReplaceValues(RSAMod, data), Password = localInputString });
                            }
                        }
                        catch (Exception ex) { outputString = ex.ToString(); }
                        break;

                    case Function.Delay:
                        try { Thread.Sleep(int.Parse(localInputString)); } catch { }
                        break;

                    case Function.CharAt:
                        outputString = localInputString.ToCharArray()[int.Parse(ReplaceValues(charIndex, data))].ToString();
                        break;

                    case Function.Substring:
                        outputString = localInputString.Substring(int.Parse(ReplaceValues(substringIndex, data)), int.Parse(ReplaceValues(substringLength,data)));
                        break;

                    case Function.ReverseString:
                        char[] charArray = localInputString.ToCharArray();
                        Array.Reverse(charArray);
                        outputString = new string(charArray);
                        break;

                    case Function.Trim:
                        outputString = localInputString.Trim();
                        break;

                    case Function.GetRandomUA:
                        outputString = RandomUserAgent(data.Random);
                        break;

                    case Function.AESEncrypt:
                        outputString = AESEncrypt(ReplaceValues(aesKey, data), localInputString);
                        break;

                    case Function.AESDecrypt:
                        outputString = AESDecrypt(ReplaceValues(aesKey, data), localInputString);
                        break;
                }
                
                data.Log(new LogEntry(string.Format("Executed function {0} on input {1} with outcome {2}", functionType, localInputString, outputString), Colors.GreenYellow));

                // Add to the outputs
                outputs.Add(outputString);
            }

            var isList = outputs.Count > 1 || InputString.Contains("[*]") || InputString.Contains("(*)") || InputString.Contains("{*}");
            InsertVariables(data, isCapture, isList, outputs, variableName, "", "", false, true);
        }

        #region Base64
        /// <summary>
        /// Encodes a string to base64.
        /// </summary>
        /// <param name="plainText">The string to encode</param>
        /// <returns>The base64-encoded string</returns>
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        /// <summary>
        /// Decodes a base64-encoded string.
        /// </summary>
        /// <param name="base64EncodedData">The base64-encoded string</param>
        /// <returns>The decoded string</returns>
        public static string Base64Decode(string base64EncodedData)
        {
            var toDecode = base64EncodedData.Replace(".", "");
            var remainder = toDecode.Length % 4;
            if (remainder != 0) toDecode = toDecode.PadRight(toDecode.Length + remainder, '=');
            var base64EncodedBytes = System.Convert.FromBase64String(toDecode);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
        #endregion

        #region Hash and Hmac
        /// <summary>
        /// Hashes a string using the specified hashing function.
        /// </summary>
        /// <param name="baseString">The string to hash</param>
        /// <param name="type">The hashing function</param>
        /// <returns>The hash digest as a hex-encoded string</returns>
        public static string GetHash(string baseString, Hash type)
        {
            switch (type)
            {
                case Hash.MD5:
                    return MD5(baseString);

                case Hash.SHA1:
                    return SHA1(baseString);

                case Hash.SHA256:
                    return SHA256(baseString);

                case Hash.SHA384:
                    return SHA384(baseString);

                case Hash.SHA512:
                    return SHA512(baseString);

                default:
                    return "UNRECOGNIZED HASH TYPE";
            }
        }

        /// <summary>
        /// Gets the HMAC signature of a message given a key and a hashing function.
        /// </summary>
        /// <param name="baseString">The message to sign</param>
        /// <param name="type">The hashing function</param>
        /// <param name="key">The HMAC key</param>
        /// <param name="base64">Whether the output should be encrypted as a base64 string</param>
        /// <returns>The HMAC signature</returns>
        public static string Hmac(string baseString, Hash type, string key, bool base64)
        {
            switch (type)
            {
                case Hash.MD5:
                    return HMACMD5(baseString, key, base64);

                case Hash.SHA1:
                    return HMACSHA1(baseString, key, base64);

                case Hash.SHA256:
                    return HMACSHA256(baseString, key, base64);

                case Hash.SHA384:
                    return HMACSHA384(baseString, key, base64);

                case Hash.SHA512:
                    return HMACSHA512(baseString, key, base64);

                default:
                    return "UNRECOGNIZED HASH TYPE";
            }
        }

        private static string MD5(string input)
        {
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hash)
                sb.Append(b.ToString("X2"));
            return sb.ToString();
        }

        private static string HMACMD5(string input, string key, bool base64)
        {
            HMACMD5 hmac = new HMACMD5(System.Text.Encoding.ASCII.GetBytes(key));
            var hash = hmac.ComputeHash(System.Text.Encoding.ASCII.GetBytes(input));
            if (base64) { return Convert.ToBase64String(hash); }
            else { return ToHex(hash); }
        }

        private static string SHA1(string input)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder(hash.Length * 2);
                foreach (byte b in hash)
                    sb.Append(b.ToString("X2"));
                return sb.ToString();
            }
        }

        private static string HMACSHA1(string input, string key, bool base64)
        {
            HMACSHA1 hmac = new HMACSHA1(System.Text.Encoding.ASCII.GetBytes(key));
            var hash = hmac.ComputeHash(System.Text.Encoding.ASCII.GetBytes(input));
            if (base64) { return Convert.ToBase64String(hash); }
            else { return ToHex(hash); }
        }

        private static string SHA256(string input)
        {
            using (SHA256Managed sha256 = new SHA256Managed())
            {
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder(hash.Length * 2);
                foreach (byte b in hash)
                    sb.Append(b.ToString("X2"));
                return sb.ToString();
            }
        }

        private static string HMACSHA256(string input, string key, bool base64)
        {
            HMACSHA256 hmac = new HMACSHA256(System.Text.Encoding.ASCII.GetBytes(key));
            var hash = hmac.ComputeHash(System.Text.Encoding.ASCII.GetBytes(input));
            if (base64) { return Convert.ToBase64String(hash); }
            else { return ToHex(hash); }
        }

        private static string SHA384(string input)
        {
            using (SHA384Managed sha384 = new SHA384Managed())
            {
                var hash = sha384.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder(hash.Length * 2);
                foreach (byte b in hash)
                    sb.Append(b.ToString("X2"));
                return sb.ToString();
            }
        }

        private static string HMACSHA384(string input, string key, bool base64)
        {
            HMACSHA384 hmac = new HMACSHA384(System.Text.Encoding.ASCII.GetBytes(key));
            var hash = hmac.ComputeHash(System.Text.Encoding.ASCII.GetBytes(input));
            if (base64) { return Convert.ToBase64String(hash); }
            else { return ToHex(hash); }
        }

        private static string SHA512(string input)
        {
            using (SHA512Managed sha512 = new SHA512Managed())
            {
                var hash = sha512.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder(hash.Length * 2);
                foreach (byte b in hash)
                    sb.Append(b.ToString("X2"));
                return sb.ToString();
            }
        }

        private static string HMACSHA512(string input, string key, bool base64)
        {
            HMACSHA512 hmac = new HMACSHA512(System.Text.Encoding.ASCII.GetBytes(key));
            var hash = hmac.ComputeHash(System.Text.Encoding.ASCII.GetBytes(input));
            if (base64) { return Convert.ToBase64String(hash); }
            else { return ToHex(hash); }
        }

        private static string ToHex(byte[] bytes)
        {
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
                sb.Append(b.ToString("X2"));
            return sb.ToString();
        }

        #endregion

        #region RSA
        private string RSAEncrypt(string input, int size)
        {
            var data = Encoding.UTF8.GetBytes(input);

            using (var rsa = new RSACryptoServiceProvider(size))
            {
                try
                {              
                    rsa.FromXmlString(input);
                    var encryptedData = rsa.Encrypt(data, true);
                    Console.WriteLine(Convert.ToString(encryptedData));
                    return Convert.ToString(encryptedData);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return "";
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }
        }

        private string RSADecrypt(string input, int size)
        {
            var data = Encoding.UTF8.GetBytes(input);

            using (var rsa = new RSACryptoServiceProvider(size))
            {
                try
                {
                    rsa.FromXmlString(input);
                    var resultBytes = System.Text.Encoding.ASCII.GetBytes(input);
                    var decryptedBytes = rsa.Decrypt(resultBytes, true);
                    return Encoding.UTF8.GetString(decryptedBytes);
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }
        }

        
        private string SteamRSAEncrypt(RsaParameters rsaParam)
        {
            // Convert the public keys to BigIntegers
            var modulus = CreateBigInteger(rsaParam.Modulus);
            var exponent = CreateBigInteger(rsaParam.Exponent);

            // (modulus.ToByteArray().Length - 1) * 8
            //modulus has 256 bytes multiplied by 8 bits equals 2048
            var encryptedNumber = Pkcs1Pad2(rsaParam.Password, (2048 + 7) >> 3);

            // And now, the RSA encryption
            encryptedNumber = System.Numerics.BigInteger.ModPow(encryptedNumber, exponent, modulus);

            //Reverse number and convert to base64
            var encryptedString = Convert.ToBase64String(encryptedNumber.ToByteArray().Reverse().ToArray());

            return encryptedString;
        }

        private static System.Numerics.BigInteger CreateBigInteger(string hex)
        {
            return System.Numerics.BigInteger.Parse("00" + hex, NumberStyles.AllowHexSpecifier);
        }

        
        private static System.Numerics.BigInteger Pkcs1Pad2(string data, int keySize)
        {
            if (keySize < data.Length + 11)
                return new System.Numerics.BigInteger();

            var buffer = new byte[256];
            var i = data.Length - 1;

            while (i >= 0 && keySize > 0)
            {
                buffer[--keySize] = (byte)data[i--];
            }

            // Padding, I think
            var random = new Random();
            buffer[--keySize] = 0;
            while (keySize > 2)
            {
                buffer[--keySize] = (byte)random.Next(1, 256);
                //buffer[--keySize] = 5;
            }

            buffer[--keySize] = 2;
            buffer[--keySize] = 0;

            Array.Reverse(buffer);

            return new System.Numerics.BigInteger(buffer);
        }

        #endregion

        #region Translation

        /// <summary>
        /// Builds a string containing translation keys.
        /// </summary>
        /// <returns>One translation key per line, with name and value separated by a colon</returns>
        public string GetDictionary()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var pair in TranslationDictionary)
            {
                sb.Append($"{pair.Key}: {pair.Value}");
                if (!pair.Equals(TranslationDictionary.Last())) sb.Append(Environment.NewLine);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Sets translation keys from an array of lines.
        /// </summary>
        /// <param name="lines">The lines containing the colon-separated name and value of the translation keys</param>
        public void SetDictionary(string[] lines)
        {
            TranslationDictionary.Clear();
            foreach (var line in lines)
            {
                if (line.Contains(':'))
                {
                    var split = line.Split(new[] { ':' }, 2);
                    var key = split[0];
                    var val = split[1].TrimStart();
                    TranslationDictionary[key] = val;
                }
            }
        }
        #endregion

        #region Count Occurrences
        /// <summary>
        /// Counts how many times a string occurs inside another string.
        /// </summary>
        /// <param name="input">The long string</param>
        /// <param name="text">The text to search</param>
        /// <returns>How many times the text appears in the long string</returns>
        public static int CountStringOccurrences(string input, string text)
        {
            // Loop through all instances of the string 'text'.
            int count = 0;
            int i = 0;
            while ((i = input.IndexOf(text, i)) != -1)
            {
                i += text.Length;
                count++;
            }
            return count;
        }
        #endregion

        #region RandomString
        private string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }
        #endregion

        #region AES
        /// <summary>
        /// Encrypts a string with AES.
        /// </summary>
        /// <param name="key">The encryption key</param>
        /// <param name="data">The data to encrypt</param>
        /// <returns>The AES-encrypted string</returns>
        public static string AESEncrypt(string key, string data)
        {
            string encData = null;
            byte[][] keys = GetHashKeys(key);

            try
            {
                encData = EncryptStringToBytes_Aes(data, keys[0], keys[1]);
            }
            catch (CryptographicException) { }
            catch (ArgumentNullException) { }

            return encData;
        }

        /// <summary>
        /// Decrypts an AES-encrypted string.
        /// </summary>
        /// <param name="key">The decryption key</param>
        /// <param name="data">The AES-encrypted data</param>
        /// <returns>The plaintext string</returns>
        public static string AESDecrypt(string key, string data)
        {
            string decData = null;
            byte[][] keys = GetHashKeys(key);

            try
            {
                decData = DecryptStringFromBytes_Aes(data, keys[0], keys[1]);
            }
            catch (CryptographicException) { }
            catch (ArgumentNullException) { }

            return decData;
        }

        private static byte[][] GetHashKeys(string key)
        {
            byte[][] result = new byte[2][];
            Encoding enc = Encoding.UTF8;

            SHA256 sha2 = new SHA256CryptoServiceProvider();

            byte[] rawKey = enc.GetBytes(key);
            byte[] rawIV = enc.GetBytes(key);

            byte[] hashKey = sha2.ComputeHash(rawKey);
            byte[] hashIV = sha2.ComputeHash(rawIV);

            Array.Resize(ref hashIV, 16);

            result[0] = hashKey;
            result[1] = hashIV;

            return result;
        }

        private static string EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            byte[] encrypted;

            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt =
                            new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            return Convert.ToBase64String(encrypted);
        }

        private static string DecryptStringFromBytes_Aes(string cipherTextString, byte[] Key, byte[] IV)
        {
            byte[] cipherText = Convert.FromBase64String(cipherTextString);

            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            string plaintext = null;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt =
                            new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return plaintext;
        }
        #endregion

        #region RandomUA

        // All credits for this method goes to the Leaf.xNet fork of Extreme.NET
        // https://github.com/csharp-leaf/Leaf.xNet

        /// <summary>
        /// Gets a random User-Agent header.
        /// </summary>
        /// <param name="rand">A random number generator</param>
        /// <returns>A randomly generated User-Agent header</returns>
        public static string RandomUserAgent(Random rand)
        {
            int random = rand.Next(99) + 1;

            // Chrome = 70%
            if (random >= 1 && random <= 70)
                return Http.ChromeUserAgent();

            // Firefox = 15%
            if (random > 70 && random <= 85)
                return Http.FirefoxUserAgent();

            // IE = 6%
            if (random > 85 && random <= 91)
                return Http.IEUserAgent();

            // Opera 12 = 5%
            if (random > 91 && random <= 96)
                return Http.OperaUserAgent();

            // Opera mini = 4%
            return Http.OperaMiniUserAgent();
        }
        #endregion
    }

    /// <summary>
    /// The RSA parameters.
    /// </summary>
    struct RsaParameters
    {
        /// <summary>The key exponent.</summary>
        public string Exponent;

        /// <summary>The key modulus.</summary>
        public string Modulus;

        /// <summary>The encryption password.</summary>
        public string Password;
    }
}
