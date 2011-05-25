using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace NotationalFerocity.WPF
{
    public class NoteRichTextBox : RichTextBox
    {
        public NoteRichTextBox()
        {
            FontSelection = FontSelection.Proportional;
        }
        
        public static readonly DependencyProperty FontSelectionProperty =
            DependencyProperty.Register("FontSelection", typeof (FontSelection), typeof (NoteRichTextBox), new PropertyMetadata(default(FontSelection)));

        public FontSelection FontSelection
        {
            get
            {
                return (FontSelection) GetValue(FontSelectionProperty);
            }
            set
            {
                SetValue(FontSelectionProperty, value);
            }
        }


        /// <summary>
        /// A convenience function to get the contents of the RichTextBox
        /// </summary>
        /// <returns>The contents of the RichTextBox</returns>
        public string GetText()
        {
            return new TextRange(Document.ContentStart,
                                 Document.ContentEnd).Text;
        }
    }
}