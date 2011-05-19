using System;
using System.Windows;
using System.Windows.Forms;

namespace NotationalFerocity.Windows
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void browseButton_Click(object sender, RoutedEventArgs e)
        {
            var folder = new FolderBrowserDialog
            {
                ShowNewFolderButton = true,
                RootFolder = Environment.SpecialFolder.MyDocuments
            };
            
            var result = folder.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                notesDirectoryTextBox.Text = folder.SelectedPath;
            }
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement Settings save logic

            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            notesDirectoryTextBox.Text = Properties.Settings.Default.NotesDirectory;

            var extensions = new string[Properties.Settings.Default.Extensions.Count];

            Properties.Settings.Default.Extensions.CopyTo(extensions, 0);

            extensionsTextBox.Text = String.Join("; ", extensions);
        }

        private void notesDirectoryTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            // TODO: Implement validation
        }

        private void extensionsTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            // TODO: Implement validation
        }
    }
}