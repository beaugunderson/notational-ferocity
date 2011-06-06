using MarkdownSharp;

using NotationalFerocity.Formatting;
using NotationalFerocity.Models;
using NotationalFerocity.Properties;
using NotationalFerocity.WPF;

namespace NotationalFerocity.Windows
{
    /// <summary>
    /// Interaction logic for OutputWindow.xaml
    /// </summary>
    public partial class OutputWindow
    {
        public Note Note { get; set; }
        public OutputType OutputType { get; set; }

        public OutputWindow(Note note, OutputType outputType)
        {
            Note = note;
            OutputType = outputType;

            InitializeComponent();
        }

        public OutputWindow()
        {
            InitializeComponent();
        }

        private string wrapOutput(string output)
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
                output,
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
            if (Note == null)
            {
                return;
            }

            string html;

            switch (OutputType)
            {
                case OutputType.Markdown:
                    html = wrapOutput(new Markdown().Transform(Note.Text));

                    break;
                case OutputType.Textile:
                    html = wrapOutput(Textile.TextileFormatter.FormatString(Note.Text));
                    
                    break;
                default:
                    html = "No formatter chosen.";
                    
                    break;
            }

            outputWebBrowser.NavigateToString(html);
        }

        private void Window_SourceInitialized(object sender, System.EventArgs e)
        {
            IconHelper.RemoveIcon(this);
        }
    }
}