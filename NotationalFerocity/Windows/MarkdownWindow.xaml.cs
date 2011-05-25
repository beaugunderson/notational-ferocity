using MarkdownSharp;
using NotationalFerocity.Models;

namespace NotationalFerocity.Windows
{
    /// <summary>
    /// Interaction logic for MarkdownWindow.xaml
    /// </summary>
    public partial class MarkdownWindow
    {
        public Note Note
        {
            get;
            set;
        }

        public MarkdownWindow(Note note)
        {
            Note = note;

            InitializeComponent();
        }

        public MarkdownWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var markdown = new Markdown();

            markdownWebBrowser.NavigateToString(markdown.Transform(Note.Text));
        }
    }
}