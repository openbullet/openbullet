using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;

namespace OpenBullet
{
    /// <summary>
    /// Logica di interazione per DialogLSDoc.xaml
    /// </summary>
    public partial class DialogLSDoc : Page
    {
        XmlNode main, currentSection, currentItem;
        XmlNodeList sections, items;

        public DialogLSDoc()
        {
            InitializeComponent();

            contentDisplay.TextArea.Foreground = new SolidColorBrush(Colors.Gainsboro);

            using (XmlReader reader = XmlReader.Create("LSHighlighting.xshd"))
                contentDisplay.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            

            XmlDocument doc = new XmlDocument();
            try { doc.Load("LSDoc.xml"); } catch { MessageBox.Show("No documentation file found!"); return; }

            // Go to the doc node
            main = doc.DocumentElement.SelectSingleNode("/doc");

            // Add all sections to the ComboBox
            sectionComboBox.Items.Clear();
            sections = main.ChildNodes;
            foreach(XmlNode s in sections)
                sectionComboBox.Items.Add(s.Attributes["name"].Value);
            sectionComboBox.SelectedIndex = 0;

            // Set the 1st as current
            currentSection = sections[0];

            SwitchPage();
        }

        private void sectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                currentSection = sections.Item(((ComboBox)e.OriginalSource).SelectedIndex);
                SwitchPage();
            }
            catch { }
        }

        private void SwitchPage()
        {
            // Get the menu items and add them as clickable Labels to the menu
            items = currentSection.ChildNodes;
            menuPanel.Children.Clear();
            foreach (XmlNode i in items)
            {
                Label label = new Label();
                label.Content = i.Attributes["name"].Value;
                label.FontWeight = FontWeights.Bold;
                label.MouseDown += menuItem_Clicked;
                menuPanel.Children.Add(label);
            }
        }

        private void menuItem_Clicked(object sender, MouseButtonEventArgs e)
        {
            try
            {
                int i;
                for (i = 0; i < items.Count; i++)
                {
                    if (items[i].Attributes["name"].Value == ((TextBlock)e.OriginalSource).Text)
                    {
                        currentItem = items[i];
                        break;
                    }
                }

                DisplayContent();
            }
            catch { }
        }

        private void DisplayContent()
        {
            titleLabel.Content = currentItem.Attributes["name"];
            contentDisplay.Text = currentItem.InnerText;
        }
    }
}
