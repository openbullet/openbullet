using OpenBullet.ViewModels;
using OpenBullet.Views.Main.Configs;
using OpenBullet.Views.Main.Runner;
using RuriLib.Models;
using RuriLib.Runner;
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
            answerTextbox.Focus();
        }
        
        private void acceptButton_Click(object sender, RoutedEventArgs e)
        {
            if(Caller.GetType() == typeof(StackerViewModel))
            {
                ((StackerViewModel)Caller).BotData.Variables.Set(new CVar(VariableName, answerTextbox.Text));
            }
            else if(Caller.GetType() == typeof(RunnerViewModel))
            {
                ((RunnerViewModel)Caller).CustomInputs.Add(new KeyValuePair<string, string>(VariableName, answerTextbox.Text));
            }
            ((MainDialog)Parent).Close();
        }

        private void answerTextbox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Return)
            {
                acceptButton_Click(sender, null);
            }
        }
    }
}
