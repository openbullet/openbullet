using Extreme.Net;
using OpenBullet.Pages.StackerBlocks;
using RuriLib;
using RuriLib.LS;
using RuriLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;

namespace OpenBullet.ViewModels
{
    public enum StackerView
    {
        LoliScript,
        Blocks
    }

    public class StackerViewModel : ViewModelBase
    {
        Random rand = new Random();

        // DEBUGGER LOGIC
        public StackerView View { get; set; } = StackerView.LoliScript;
        public BotData BotData { get; set; }
        public LoliScript LS { get; set; }

        private bool controlsEnabled = true;
        public bool ControlsEnabled { get { return controlsEnabled; } set { controlsEnabled = value; OnPropertyChanged("ControlsEnabled"); } }

        // CURRENT CONFIG LOGIC AND CONTROLS
        public ConfigViewModel Config { get; set; }

        public ObservableCollection<StackerBlockViewModel> Stack { get; set; } = new ObservableCollection<StackerBlockViewModel>();
        public List<StackerBlockViewModel> SelectedBlocks { get { return Stack.Where(b => b.Selected).ToList(); } }

        public StackerBlockViewModel CurrentBlock { get; set; } = null;
        public int CurrentBlockIndex { get { return Stack.IndexOf(CurrentBlock); } }

        public void DeselectAll() { foreach (var b in Stack) b.Selected = false; }
        public void SelectAll() { foreach (var b in Stack) b.Selected = true; }

        public BlockBase LastDeletedBlock { get; set; }
        public int LastDeletedIndex { get; set; }

        public List<BlockBase> GetList()
        {
            ConvertKeychains();
            return Stack.Select(x => x.Block).ToList();
        }

        public void ConvertKeychains()
        {
            // Convert the keychains (TODO: Find out why it's not saving them automatically from the ItemControl inside the Page)
            foreach (StackerBlockViewModel stackerBlock in Stack)
            {
                var block = stackerBlock.Block;
                var page = stackerBlock.Page;
                if (page.GetType() == typeof(PageBlockKeycheck))
                {
                    var kcpage = (PageBlockKeycheck)page;
                    var kcblock = (BlockKeycheck)block;

                    kcblock.KeyChains = kcpage.vm.KeychainList.Select(k => k.Keychain).ToList();
                }
            }
        }

        public StackerBlockViewModel GetBlockById(int id)
        {
            return Stack.Where(x => x.Id == id).First();
        }

        public void AddBlock(BlockBase block, int index = -1)
        {
            if (index < 0 || index > Stack.Count) index = Stack.Count;
            Stack.Insert(index, new StackerBlockViewModel(block, rand));
            UpdateHeights();
        }

        public void ClearBlocks()
        {
            Stack.Clear();
            UpdateHeights();
        }

        public void UpdateHeights()
        {
            foreach (var s in Stack)
                s.UpdateHeight(Clamp((600 / Stack.Count), 30, 60));
        }

        public void MoveBlockUp(StackerBlockViewModel block)
        {
            var oldIndex = Stack.IndexOf(block);
            if(oldIndex != 0)
                Stack.Move(oldIndex, oldIndex - 1);
        }

        public void MoveBlockDown(StackerBlockViewModel block)
        {
            var oldIndex = Stack.IndexOf(block);
            if (oldIndex != Stack.Count - 1)
                Stack.Move(oldIndex, oldIndex + 1);
        }

        public int Clamp(int a, int min, int max)
        {
            if (a < min)
                return min;
            else if (a > max)
                return max;
            else
                return a;
        }

        // DEBUGGER CONTROLS
        private ProxyType proxyType = ProxyType.Http;
        public ProxyType ProxyType { get { return proxyType; } set { proxyType = value; OnPropertyChanged(); } }

        private string testData = "";
        public string TestData { get { return testData; } set { testData = value; OnPropertyChanged(); } }

        private string testDataType = "";
        public string TestDataType { get { return testDataType; } set { testDataType = value; OnPropertyChanged(); } }

        private string testProxy = "";
        public string TestProxy { get { return testProxy; } set { testProxy = value; OnPropertyChanged(); } }

        private bool useProxy = false;
        public bool UseProxy { get { return useProxy; } set { useProxy = value; OnPropertyChanged(); OnPropertyChanged("UseProxyString"); OnPropertyChanged("UseProxyColor"); } }
        public string UseProxyString { get { return UseProxy ? "ON" : "OFF"; } }
        public SolidColorBrush UseProxyColor { get { return UseProxy ? Globals.GetBrush("ForegroundGood") : Globals.GetBrush("ForegroundBad"); } }

        private bool sbs = false;
        public bool SBS { get { return sbs; } set { sbs = value; OnPropertyChanged(); } }

        private bool sbsClear = false;
        public bool SBSClear { get { return sbsClear; } set { sbsClear = value; OnPropertyChanged(); } }

        private bool sbsEnabled = false;
        public bool SBSEnabled { get { return sbsEnabled; } set { sbsEnabled = value; OnPropertyChanged(); } }

        // Search
        private string searchString = "";
        public string SearchString { get { return searchString; } set { searchString = value; OnPropertyChanged(); OnPropertyChanged("SearchProgress"); } }

        private List<int> indexes = new List<int>();
        public List<int> Indexes { get { return indexes; } set { indexes = value; OnPropertyChanged("TotalSearchMatches"); OnPropertyChanged("CurrentSearchMatch"); } }
        public int TotalSearchMatches { get { return Indexes.Count; } }

        public void UpdateTotalSearchMatches()
        {
            OnPropertyChanged("TotalSearchMatches");
        }

        private int currentSearchMatch = 0;
        public int CurrentSearchMatch { get { return currentSearchMatch; } set { currentSearchMatch = value; OnPropertyChanged(); } }

        public StackerViewModel(ConfigViewModel config)
        {
            Config = config;
        }
    }
}
