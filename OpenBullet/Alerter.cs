using RuriLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OpenBullet
{
    public class Alerter : IAlerter
    {
        public bool YesOrNo(string message, string title)
        {
            return MessageBox.Show(message, title, MessageBoxButton.YesNo) == MessageBoxResult.Yes;
        }
    }
}
