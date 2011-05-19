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

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            notesDirectoryTextBox.Text = Properties.Settings.Default.NotesDirectory;
            extensionsTextBox.Text = String.Join("; ", Properties.Settings.Default.Extensions);
        }

        private void notesDirectoryTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void extensionsTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }
    }
}
