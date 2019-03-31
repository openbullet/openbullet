using RuriLib.LS;
using System;
using System.Windows.Media;

namespace RuriLib
{
    /// <summary>
    /// A block that executes javascript code in the selenium-driven browser.
    /// </summary>
    public class SBlockExecuteJS : BlockBase
    {
        private string javascriptCode = "alert('henlo');";
        /// <summary>The javascript code.</summary>
        public string JavascriptCode { get { return javascriptCode; } set { javascriptCode = value; OnPropertyChanged(); } }

        /// <summary>
        /// Creates an ExecuteJS block.
        /// </summary>
        public SBlockExecuteJS()
        {
            Label = "EXECUTE JS";
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
             * EXECUTEJS "SCRIPT"
             * */

            JavascriptCode = LineParser.ParseLiteral(ref input, "SCRIPT");

            return this;
        }

        /// <inheritdoc />
        public override string ToLS(bool indent = true)
        {
            var writer = new BlockWriter(GetType(), indent, Disabled);
            writer
                .Label(Label)
                .Token("EXECUTEJS")
                .Literal(JavascriptCode.Replace("\r\n", " ").Replace("\n", " "));
            return writer.ToString();
        }

        /// <inheritdoc />
        public override void Process(BotData data)
        {
            base.Process(data);

            if (data.Driver == null)
            {
                data.Log(new LogEntry("Open a browser first!", Colors.White));
                throw new Exception("Browser not open");
            }

            data.Log(new LogEntry("Executing JS code!", Colors.White));
            data.Driver.ExecuteScript(ReplaceValues(javascriptCode, data));
            data.Log(new LogEntry("... executed!", Colors.White));

            UpdateSeleniumData(data);
        }
    }
}
