using Extreme.Net;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using OpenBullet.ViewModels;
using RuriLib;
using RuriLib.LS;
using RuriLib.Models;
using RuriLib.Runner;
using RuriLib.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;

namespace OpenBullet
{
    /// <summary>
    /// Logica di interazione per Stacker.xaml
    /// </summary>
    /// 
    public partial class Stacker : Page
    {
        private DateTime startTime;
        public StackerViewModel vm;
        private AbortableBackgroundWorker debugger = new AbortableBackgroundWorker();
        XmlNodeList syntaxHelperItems;
        TextEditor toolTipEditor;
        private ToolTip toolTip;
        public delegate void SaveConfigEventHandler(object sender, EventArgs e);
        public event SaveConfigEventHandler SaveConfig;
        protected virtual void OnSaveConfig()
        {
            SaveConfig?.Invoke(this, EventArgs.Empty);
        }

        public Stacker(ConfigViewModel config)
        {
            InitializeComponent();
            vm = new StackerViewModel(config);
            DataContext = vm;

            // Style the LoliScript editor
            loliScriptEditor.ShowLineNumbers = true;
            loliScriptEditor.TextArea.Foreground = new SolidColorBrush(Colors.Gainsboro);
            loliScriptEditor.TextArea.TextView.LinkTextForegroundBrush = new SolidColorBrush(Colors.DodgerBlue);
            using (XmlReader reader = XmlReader.Create("LSHighlighting.xshd"))
            {
                loliScriptEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            }

            // Load the Syntax Helper XML
            XmlDocument doc = new XmlDocument();
            try {

                doc.Load("SyntaxHelper.xml");
                var main = doc.DocumentElement.SelectSingleNode("/doc");
                syntaxHelperItems = main.ChildNodes;

                // Only bind the keydown event if the XML was successfully loaded
                loliScriptEditor.KeyDown += loliScriptEditor_KeyDown;
            }
            catch { }

            // Make the Avalon Editor for Syntax Helper and style it
            toolTipEditor = new TextEditor();
            toolTipEditor.TextArea.Foreground = Globals.GetBrush("ForegroundMain");
            toolTipEditor.Background = new SolidColorBrush(Color.FromArgb(22, 22, 22, 50));
            toolTipEditor.TextArea.TextView.LinkTextForegroundBrush = new SolidColorBrush(Colors.DodgerBlue);
            toolTipEditor.FontSize = 11;
            toolTipEditor.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            toolTipEditor.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            using (XmlReader reader = XmlReader.Create("LSHighlighting.xshd"))
            {
                toolTipEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            }

            toolTip = new ToolTip { Placement = PlacementMode.Relative, PlacementTarget = loliScriptEditor };
            toolTip.Content = toolTipEditor;
            loliScriptEditor.ToolTip = toolTip;

            // Load the script
            vm.LS = new LoliScript(config.Config.Script);
            loliScriptEditor.Text = vm.LS.Script;

            // If the user prefers Stack view, switch to it
            if (!Globals.obSettings.General.DisplayLoliScriptOnLoad)
            {
                stackButton_Click(this, null);
            }

            // Style the logRTB
            logRTB.Font = new System.Drawing.Font("Consolas", 10);
            logRTB.BackColor = System.Drawing.Color.FromArgb(22, 22, 22);

            // Hook the context menu to the logRTB
            System.Windows.Forms.ContextMenu menu = new System.Windows.Forms.ContextMenu();
            System.Windows.Forms.MenuItem item = new System.Windows.Forms.MenuItem();
            item.Text = "Clear";
            item.Click += ClearDebuggerLog;
            menu.MenuItems.Add(item);
            logRTB.ContextMenu = menu;
            logRTB.MouseClick += DebuggerLogRightClick;

            foreach (string i in Enum.GetNames(typeof(ProxyType)))
                if (i != "Chain") proxyTypeCombobox.Items.Add(i);

            proxyTypeCombobox.SelectedIndex = 0;

            foreach (var t in Globals.environment.GetWordlistTypeNames())
                testDataTypeCombobox.Items.Add(t);

            testDataTypeCombobox.SelectedIndex = 0;

            // Initialize debugger
            debugger.WorkerSupportsCancellation = true;
            debugger.Status = WorkerStatus.Idle;
            debugger.DoWork += new DoWorkEventHandler(debuggerCheck);
            debugger.RunWorkerCompleted += new RunWorkerCompletedEventHandler(debuggerCompleted);

            this.SaveConfig += Globals.mainWindow.ConfigsPage.ConfigManagerPage.OnSaveConfig;
        }

        private void ClearDebuggerLog(object sender, EventArgs e)
        {
            logRTB.Clear();
        }

        private void DebuggerLogRightClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Right)
            {
                return;
            }

            var menu = ((System.Windows.Forms.RichTextBox)sender).ContextMenu;
            menu.Show(logRTB, e.Location);
        }

        #region Buttons
        private void Image_MouseEnter(object sender, MouseEventArgs e)
        {
            ((Image)e.OriginalSource).Width = 25;
            ((Image)e.OriginalSource).Height = 25;
        }

        private void Image_MouseLeave(object sender, MouseEventArgs e)
        {
            ((Image)e.OriginalSource).Width = 20;
            ((Image)e.OriginalSource).Height = 20;
        }

        private void AddBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            (new MainDialog(new DialogAddBlock(this), "Add Block")).ShowDialog();
        }

        public void AddBlock(BlockBase block)
        {
            Globals.LogInfo(Components.Stacker, $"Added a block of type {block.GetType()} in position {vm.Stack.Count}");
            vm.AddBlock(block, -1);
        }

        private void RemoveBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // TODO: Bring back Ctrl+Z stuff
            //vm.LastDeletedBlock = IOManager.DuplicateBlock(vm.CurrentBlock.Block);
            //vm.LastDeletedIndex = vm.Stack.IndexOf(vm.CurrentBlock);

            foreach (var block in vm.SelectedBlocks) vm.Stack.Remove(block);
            vm.CurrentBlock = null;
            BlockInfo.Content = null;
            vm.UpdateHeights();
        }

        private void DisableBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            foreach (var b in vm.SelectedBlocks) b.Disable();
        }

        private void CloneBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            vm.ConvertKeychains();
            foreach(var block in vm.SelectedBlocks)
                vm.AddBlock(IOManager.CloneBlock(block.Block), vm.Stack.IndexOf(block) + 1);
        }

        private void MoveUpBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            foreach (var block in vm.SelectedBlocks) vm.MoveBlockUp(block);
        }

        private void MoveDownBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            foreach (var block in vm.SelectedBlocks.AsEnumerable().Reverse()) vm.MoveBlockDown(block);
        }

        private void SaveConfig_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OnSaveConfig();
        }
        #endregion

        #region Keyboard Events
        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case System.Windows.Input.Key.Z:
                        if (vm.LastDeletedBlock != null)
                        {
                            vm.AddBlock(vm.LastDeletedBlock, vm.LastDeletedIndex);
                            Globals.LogInfo(Components.Stacker, $"Readded block of type {vm.LastDeletedBlock.GetType()} in position {vm.LastDeletedIndex}");
                            vm.LastDeletedBlock = null;
                        }
                        else Globals.LogError(Components.Stacker, "Nothing to undo");
                        break;

                    case System.Windows.Input.Key.C:
                        try { Clipboard.SetText(IOManager.SerializeBlocks(vm.SelectedBlocks.Select(b => b.Block).ToList())); }
                        catch { Globals.LogError(Components.Stacker, "Exception while copying blocks"); }
                        break;

                    case System.Windows.Input.Key.V:
                        try
                        {
                            foreach (var block in IOManager.DeserializeBlocks(Clipboard.GetText()))
                                vm.AddBlock(block);
                        }
                        catch { Globals.LogError(Components.Stacker, "Exception while pasting blocks"); }
                        break;

                    case System.Windows.Input.Key.S:
                        vm.LS.Script = loliScriptEditor.Text;
                        OnSaveConfig();
                        break;

                    default:
                        break;
                }
                
            }
        }
        #endregion

        #region Debugger
        private void startDebuggerButton_Click(object sender, RoutedEventArgs e)
        {
            switch (debugger.Status)
            {
                case WorkerStatus.Idle:
                    if (vm.View == StackerView.Blocks)
                        vm.LS.FromBlocks(vm.GetList());
                    else
                        vm.LS.Script = loliScriptEditor.Text;

                    if (debuggerTabControl.SelectedIndex == 1)
                        logRTB.Focus();
                    vm.ControlsEnabled = false;
                    if (!Globals.obSettings.General.PersistDebuggerLog)
                        logRTB.Clear();
                    dataRTB.Document.Blocks.Clear();

                    if (!debugger.IsBusy)
                    {
                        debugger.RunWorkerAsync();
                        Globals.LogInfo(Components.Stacker, "Started the debugger");
                    }
                    else { Globals.LogError(Components.Stacker, "Cannot start the debugger (busy)"); }

                    startDebuggerButton.Content = "Abort";
                    debugger.Status = WorkerStatus.Running;
                    break;

                case WorkerStatus.Running:
                    if (debugger.IsBusy)
                    {
                        debugger.CancelAsync();
                        Globals.LogInfo(Components.Stacker, "Sent Cancellation Request to the debugger");
                    }
                        
                    startDebuggerButton.Content = "Force";
                    debugger.Status = WorkerStatus.Stopping;
                    break;

                case WorkerStatus.Stopping:
                    debugger.Abort();
                    Globals.LogInfo(Components.Stacker, "Hard aborted the debugger");
                    startDebuggerButton.Content = "Start";
                    debugger.Status = WorkerStatus.Idle;
                    vm.ControlsEnabled = true;
                    break;
            }
        }

        private void debuggerCheck(object sender, DoWorkEventArgs e)
        {
            // Dispose of previous browser (if any)
            if(vm.BotData != null)
            {
                if (vm.BotData.BrowserOpen)
                {
                    Globals.LogInfo(Components.Stacker, "Quitting the previously opened browser");
                    vm.BotData.Driver.Quit();
                    Globals.LogInfo(Components.Stacker, "Quitted correctly");
                }
            }

            // Convert Observables
            Globals.LogInfo(Components.Stacker, "Converting Observables");
            vm.ConvertKeychains();

            // Initialize Request Data
            Globals.LogInfo(Components.Stacker, "Initializing the request data");
            CProxy proxy = null;
            if (vm.TestProxy.StartsWith("(")) // Parse in advanced mode
            {
                try { proxy = (new CProxy()).Parse(vm.TestProxy); }
                catch { Globals.LogError(Components.Stacker, "Invalid Proxy Syntax", true); }
            }
            else // Parse in standard mode
            {
                proxy = new CProxy(vm.TestProxy, vm.ProxyType);
            }

            // Initialize BotData and Reset LS
            var cData = new CData(vm.TestData, Globals.environment.GetWordlistType(vm.TestDataType));

            vm.BotData = new BotData(Globals.rlSettings, vm.Config.Config.Settings, cData, proxy, vm.UseProxy);
            vm.LS.Reset();

            // Ask for user input
            foreach (var input in vm.BotData.ConfigSettings.CustomInputs)
            {
                Globals.LogInfo(Components.Stacker, $"Asking for user input: {input.Description}");
                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    (new MainDialog(new DialogCustomInput(this, input.VariableName, input.Description), "Custom Input")).ShowDialog();
                }));
            }

            // Set start block
            Globals.LogInfo(Components.Stacker, "Setting the first block as the current block");

            // Print start line
            vm.BotData.LogBuffer.Add(new LogEntry(string.Format("===== DEBUGGER STARTED FOR CONFIG {0} WITH DATA {1} AND PROXY {2} ({3}) ====="+Environment.NewLine, vm.Config.Name, vm.TestData, vm.TestProxy, vm.UseProxy ? "ENABLED" : "DISABLED"), Colors.White));
            startTime = DateTime.Now;

            // Open browser if Always Open
            if (vm.Config.Config.Settings.AlwaysOpen)
            {
                Globals.LogInfo(Components.Stacker, "Opening the Browser");
                SBlockBrowserAction.OpenBrowser(vm.BotData);
            }

            // Step-by-step
            if (vm.SBS)
            {
                vm.SBSClear = true; // Good to go for the first round
                do
                {
                    Thread.Sleep(100);

                    if (debugger.CancellationPending) {
                        Globals.LogInfo(Components.Stacker, "Found cancellation pending, aborting debugger");
                        return;
                    }

                    if (vm.SBSClear)
                    {
                        vm.SBSEnabled = false;
                        Process();
                        Globals.LogInfo(Components.Stacker, $"Block processed in SBS mode, can proceed: {vm.LS.CanProceed}");
                        vm.SBSEnabled = true;
                        vm.SBSClear = false;
                    }
                }
                while (vm.LS.CanProceed);
            }
            
            // Normal
            else
            {
                do
                {
                    if (debugger.CancellationPending)
                    {
                        Globals.LogInfo(Components.Stacker, "Found cancellation pending, aborting debugger");
                        return;
                    }

                    Process();
                }
                while (vm.LS.CanProceed);
            }

            // Quit Browser if Always Quit
            if (vm.Config.Config.Settings.AlwaysQuit)
            {
                try {
                    vm.BotData.Driver.Quit();
                    vm.BotData.BrowserOpen = false;
                    Globals.LogInfo(Components.Stacker, "Successfully quit the browser");
                }
                catch (Exception ex) { Globals.LogError(Components.Stacker, $"Cannot quit the browser - {ex.Message}"); }
            }
        }
        
        private void Process()
        {
            try {
                vm.LS.TakeStep(vm.BotData);
                Globals.LogInfo(Components.Stacker, $"Processed {BlockBase.TruncatePretty(vm.LS.CurrentLine, 20)}");
            }
            catch (Exception ex) {
                Globals.LogError(Components.Stacker, $"Processing of line {BlockBase.TruncatePretty(vm.LS.CurrentLine, 20)} failed, exception: {ex.Message}");
            }
            
            PrintBotData();
            PrintLogBuffer();
            DisplayHTML();
        }
        
        private void PrintLogBuffer()
        {
            if (vm.BotData.LogBuffer.Count == 0) return;
            App.Current.Dispatcher.Invoke(new Action(() => {
                foreach (LogEntry entry in vm.BotData.LogBuffer)
                    logRTB.AppendText(entry.LogString, entry.LogColor);

                vm.BotData.LogBuffer.Add(new LogEntry(Environment.NewLine, Colors.White));

                try
                {
                    logRTB.SelectionStart = logRTB.TextLength;
                    logRTB.ScrollToCaret();
                }
                catch { }
            }));
        }
        
        private void PrintBotData()
        {
            App.Current.Dispatcher.Invoke(new Action(() => {
                
                dataRTB.Document.Blocks.Clear();
                dataRTB.AppendText(Environment.NewLine);
                dataRTB.AppendText($"BOT STATUS: {vm.BotData.StatusString}"+Environment.NewLine, Colors.White);
                dataRTB.AppendText("VARIABLES:" + Environment.NewLine, Colors.Yellow);
                if (Globals.obSettings.General.DisplayCapturesLast)
                {
                    foreach (var variable in vm.BotData.Variables.All.Where(v => !v.Hidden && !v.IsCapture))
                        dataRTB.AppendText(variable.Name + $" ({variable.Type}) = " + variable.ToString() + Environment.NewLine, Colors.Yellow);

                    foreach (var variable in vm.BotData.Variables.All.Where(v => !v.Hidden && v.IsCapture))
                        dataRTB.AppendText(variable.Name + $" ({variable.Type}) = " + variable.ToString() + Environment.NewLine, Colors.Tomato);
                }
                else
                {
                    foreach (var variable in vm.BotData.Variables.All.Where(v => !v.Hidden))
                        dataRTB.AppendText(variable.Name + $" ({variable.Type}) = " + variable.ToString() + Environment.NewLine, variable.IsCapture ? Colors.Tomato : Colors.Yellow);
                }
            }));
        }
        
        private void DisplayHTML()
        {
            if (Globals.obSettings.General.DisableHTMLView) return;
            App.Current.Dispatcher.Invoke(new Action(() => {
                if (vm.BotData.ResponseSource != "")
                {
                    htmlViewBrowser.DocumentText = vm.BotData.ResponseSource.Replace("alert(", "(");
                }
            }));
        }

        public void HideScriptErrors(WebBrowser wb, bool hide)
        {
            var fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fiComWebBrowser == null) return;
            var objComWebBrowser = fiComWebBrowser.GetValue(wb);
            if (objComWebBrowser == null)
            {
                wb.Loaded += (o, s) => HideScriptErrors(wb, hide); //In case we are to early
                return;
            }
            objComWebBrowser.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, objComWebBrowser, new object[] { hide });
        }
        
        private void debuggerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            debugger.Status = WorkerStatus.Idle;
            startDebuggerButton.Content = "Start";
            vm.SBSEnabled = false;
            vm.ControlsEnabled = true;

            // Print final line
            vm.BotData.LogBuffer.Clear();
            
            // Check if the input data was valid
            if (!vm.BotData.Data.IsValid)
                vm.BotData.LogBuffer.Add(new LogEntry($"WARNING: The test input data did not respect the validity regex for the selected wordlist type!", Colors.Tomato));

            if (!vm.BotData.Data.RespectsRules(vm.Config.Config.Settings.DataRules.ToList()))
                vm.BotData.LogBuffer.Add(new LogEntry($"WARNING: The test input data did not respect the data rules of this config!", Colors.Tomato));

            vm.BotData.LogBuffer.Add(new LogEntry($"===== DEBUGGER ENDED AFTER {(DateTime.Now - startTime).TotalSeconds} SECOND(S) WITH STATUS: {vm.BotData.StatusString} =====", Colors.White));
            PrintLogBuffer();
            Globals.LogInfo(Components.Stacker, "Debugger completed");
        }

        private void nextStepButton_Click(object sender, RoutedEventArgs e)
        {
            vm.SBSClear = true;
        }

        private void proxyTypeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.ProxyType = (ProxyType)proxyTypeCombobox.SelectedIndex;
        }
        
        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            Globals.LogInfo(Components.Stacker, $"Seaching for {vm.SearchString}");

            // Reset all highlights
            logRTB.SelectAll();
            logRTB.SelectionBackColor = System.Drawing.Color.FromArgb(22, 22, 22);
            logRTB.DeselectAll();

            // Check for empty search
            if (vm.SearchString == string.Empty)
                return;

            int s_start = logRTB.SelectionStart, startIndex = 0, index;
            vm.Indexes.Clear();

            while ((index = logRTB.Text.IndexOf(vm.SearchString, startIndex, StringComparison.InvariantCultureIgnoreCase)) != -1)
            {
                logRTB.Select(index, vm.SearchString.Length);
                logRTB.SelectionColor = System.Drawing.Color.White;
                logRTB.SelectionBackColor = System.Drawing.Color.Navy;

                startIndex = index + vm.SearchString.Length;
                vm.Indexes.Add(startIndex);
                if (vm.Indexes.Count == 1) logRTB.ScrollToCaret();
            }

            vm.UpdateTotalSearchMatches();

            // Reset the selection
            logRTB.SelectionStart = s_start;
            logRTB.SelectionLength = 0;
            logRTB.SelectionColor = System.Drawing.Color.Black;

            Globals.LogInfo(Components.Stacker, $"Found {vm.Indexes.Count} matches", true);

            if (vm.Indexes.Count > 0)
                vm.CurrentSearchMatch = 1;
        }

        public static List<int> AllIndexesOf(string str, string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("the string to find may not be empty", "value");
            List<int> indexes = new List<int>();
            for (int index = 0; ; index += value.Length)
            {
                index = str.IndexOf(value, index, StringComparison.InvariantCultureIgnoreCase);
                if (index == -1)
                    return indexes;
                indexes.Add(index);
            }
        }

        private void previousMatchButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (vm.CurrentSearchMatch == 1 || vm.TotalSearchMatches == 0)
                return;

            vm.CurrentSearchMatch--;
            logRTB.DeselectAll();
            logRTB.Select(vm.Indexes[vm.CurrentSearchMatch - 1], 0);
            logRTB.ScrollToCaret();
        }
        
        private void nextMatchButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (vm.CurrentSearchMatch == vm.Indexes.Count || vm.TotalSearchMatches == 0)
                return;

            vm.CurrentSearchMatch++;
            logRTB.DeselectAll();
            logRTB.Select(vm.Indexes[vm.CurrentSearchMatch - 1], 0);
            logRTB.ScrollToCaret();
        }

        private void labelTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (vm.CurrentBlock != null)
                vm.CurrentBlock.Block.Label = labelTextbox.Text;
        }

        public static TextPointer GetTextPointAt(TextPointer from, int pos)
        {
            TextPointer ret = from;
            int i = 0;

            while ((i < pos) && (ret != null))
            {
                if ((ret.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.Text) || (ret.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.None))
                    i++;

                if (ret.GetPositionAtOffset(1, LogicalDirection.Forward) == null)
                    return ret;

                ret = ret.GetPositionAtOffset(1, LogicalDirection.Forward);
            }

            return ret;
        }

        private void testDataTypeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.TestDataType = (string)testDataTypeCombobox.SelectedItem;
        }
        #endregion

        #region Block Clicked
        private void blockClicked(object sender, RoutedEventArgs e)
        {
            var toggle = sender as ToggleButton;
            var block = vm.GetBlockById((int)toggle.Tag);

            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                block.Selected = !block.Selected;
            }
            else
            {
                vm.DeselectAll();
                block.Selected = true;
            }

            vm.CurrentBlock = vm.SelectedBlocks.LastOrDefault();
            if (vm.CurrentBlock == null) return;

            if (vm.CurrentBlock.Page != null)
                BlockInfo.Content = vm.CurrentBlock.Page; // Display the Block Info page

            labelTextbox.Text = vm.CurrentBlock.Block.Label;
        }
        #endregion

        public void SetScript()
        {
            if (vm.View == StackerView.Blocks)
                vm.LS.FromBlocks(vm.GetList());
            else
                vm.LS.Script = loliScriptEditor.Text;

            vm.Config.Config.Script = vm.LS.Script;
        }

        private void loliScriptButton_Click(object sender, RoutedEventArgs e)
        {
            // Convert the Blocks to Script
            vm.LS.FromBlocks(vm.GetList());

            // Display the converted Script into the avalon editor
            loliScriptEditor.Text = vm.LS.Script;

            // Switch tab
            vm.View = StackerView.LoliScript;
            stackerTabControl.SelectedIndex = 0;
        }

        private void stackButton_Click(object sender, RoutedEventArgs e)
        {
            // Try to convert to blocks
            List<BlockBase> blocks = null;
            try
            {
                blocks = vm.LS.ToBlocks();
            }
            catch (Exception ex) {
                MessageBox.Show($"Error while converting to blocks, please check the syntax!\n{ex.Message}");
                vm.View = StackerView.LoliScript; // Make sure the view is back to LoliScript
                return;
            }

            // Add the block viewmodels to the stack
            vm.ClearBlocks();
            foreach (var block in blocks)
                vm.AddBlock(block, -1);

            // Clear the last Block Info page
            vm.CurrentBlock = null;
            BlockInfo.Content = null;

            // Switch tab
            vm.View = StackerView.Blocks;
            stackerTabControl.SelectedIndex = 1;
        }

        private void loliScriptEditor_LostFocus(object sender, RoutedEventArgs e)
        {
            vm.LS.Script = loliScriptEditor.Text;
            toolTip.IsOpen = false;
        }

        private void loliScriptEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == System.Windows.Input.Key.S)
            {
                vm.LS.Script = loliScriptEditor.Text;
                OnSaveConfig();
            }

            if (Globals.obSettings.General.DisableSyntaxHelper) return;

            DocumentLine line = loliScriptEditor.Document.GetLineByOffset(loliScriptEditor.CaretOffset);
            var blockLine = loliScriptEditor.Document.GetText(line.Offset, line.Length);

            // Scan for the first non-indented line
            while (blockLine.StartsWith(" ") || blockLine.StartsWith("\t"))
            {
                try
                {
                    line = line.PreviousLine;
                    blockLine = loliScriptEditor.Document.GetText(line.Offset, line.Length);
                }
                catch { break; }
            }

            if (BlockParser.IsBlock(blockLine))
            {
                var blockName = BlockParser.GetBlockType(blockLine);

                var caret = loliScriptEditor.TextArea.Caret.CalculateCaretRectangle();
                toolTip.HorizontalOffset = caret.Right;
                toolTip.VerticalOffset = caret.Bottom;

                XmlNode node = null;
                for (int i = 0; i < syntaxHelperItems.Count; i++)
                {
                    if (syntaxHelperItems[i].Attributes["name"].Value.ToUpper() == blockName.ToUpper())
                    {
                        node = syntaxHelperItems[i];
                        break;
                    }
                }
                if (node == null) return;

                toolTipEditor.Text = node.InnerText;
                toolTip.IsOpen = true;
            }
            else
            {
                toolTip.IsOpen = false;
            }
            
        }

        private void openDocButton_Click(object sender, RoutedEventArgs e)
        {
            (new MainDialog(new DialogLSDoc(), "LoliScript Documentation")).Show();
        }
    }

    public static class WFRichTextBoxExtensions
    {
        public static void AppendText(this System.Windows.Forms.RichTextBox box, string text, Color color)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            box.SelectionColor = System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
            box.AppendText(Environment.NewLine);
        }
    }
}
