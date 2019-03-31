using RuriLib.Models;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet
{
    /// <summary>
    /// Logica di interazione per DialogCustomInput.xaml
    /// </summary>
    public partial class DialogCustomInput : Page
    {
        object Caller { get; set; }
        string VariableName { get; set; }

        public DialogCustomInput(object caller, string variableName, string question)
        {
            InitializeComponent();
            Caller = caller;
            VariableName = variableName;
            questionTextbox.Text = question;
        }
        
        private void acceptButton_Click(object sender, RoutedEventArgs e)
        {
            if(Caller.GetType() == typeof(Stacker))
            {
                ((Stacker)Caller).vm.BotData.Variables.Set(new CVar(VariableName, answerTextbox.Text));
            }
            else if(Caller.GetType() == typeof(Runner))
            {
                ((Runner)Caller).vm.CustomInputs.Add(new KeyValuePair<string, string>(VariableName, answerTextbox.Text));
            }
            ((MainDialog)Parent).Close();
        }
    }
}
