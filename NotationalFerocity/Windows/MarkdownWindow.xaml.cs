using MarkdownSharp;
using NotationalFerocity.Models;
using NotationalFerocity.Properties;
using NotationalFerocity.WPF;

namespace NotationalFerocity.Windows
{
    /// <summary>
    /// Interaction logic for MarkdownWindow.xaml
    /// </summary>
    public partial class MarkdownWindow
    {
        public Note Note { get; set; }

        public MarkdownWindow(Note note)
        {
            Note = note;

            InitializeComponent();
        }

        public MarkdownWindow()
        {
            InitializeComponent();
        }

        private string wrapMarkdown(string markdown)
        {
            var wrapped = string.Format(
@"<html>
 <head>
  <title>{0}</title>

  <style type=""text/css"">
   body {{
      font-family: Segoe UI;
      padding: 1em;
      background-color: {2};
      color: {3};
   }}

   pre {{
      margin-left: 2em;
   }}
  </style>
 </head>

 <body>
  {1}
 </body>
</html>",
                Note.FileNameWithoutExtension,
                markdown,
                string.Format("rgb({0:d}, {1:d}, {2:d})",
                    Settings.Default.ColorBackground.Color.R,
                    Settings.Default.ColorBackground.Color.G,
                    Settings.Default.ColorBackground.Color.B),
                string.Format("rgb({0:d}, {1:d}, {2:d})",
                    Settings.Default.ColorForeground.Color.R,
                    Settings.Default.ColorForeground.Color.G,
                    Settings.Default.ColorForeground.Color.B));

            return wrapped;
        }

        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var markdown = new Markdown();

            var markedDown = wrapMarkdown(markdown.Transform(Note.Text));

            markdownWebBrowser.NavigateToString(markedDown);
        }

        private void Window_SourceInitialized(object sender, System.EventArgs e)
        {
            IconHelper.RemoveIcon(this);
        }
    }
}