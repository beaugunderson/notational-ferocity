using System;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using NotationalFerocity.WPF;

using Forms = System.Windows.Forms;
using TextBox = System.Windows.Controls.TextBox;

using FontDialog;

using NotationalFerocity.Properties;

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

            if (result == Forms.DialogResult.OK)
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
            Settings.Default.NoteFontMonospaced = FontDefinition.FromDependencyObject(monospacedFontTextBox);
            Settings.Default.NoteFontProportional = FontDefinition.FromDependencyObject(proportionalFontTextBox);

            Settings.Default.NotesDirectory = notesDirectoryTextBox.Text.Trim();

            var extensions = new StringCollection();

            foreach (var extension in Regex.Split(extensionsTextBox.Text.Trim(), @"[\s,;]"))
            {
                extensions.Add(extension);
            }

            Settings.Default.Extensions = extensions;

            Settings.Default.ColorBackground = backgroundColorPicker.SelectedColor;
            Settings.Default.ColorForeground = foregroundColorPicker.SelectedColor;

            Settings.Default.Save();
            
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Load notes directory
            notesDirectoryTextBox.Text = Settings.Default.NotesDirectory;

            // Load file extensions
            var extensions = new string[Settings.Default.Extensions.Count];

            Settings.Default.Extensions.CopyTo(extensions, 0);

            extensionsTextBox.Text = String.Join("; ", extensions);
        
            // Load monospaced font
            //monospacedFontTextBox.Text = SetFontProperties(monospacedFontTextBox, Settings.Default.NoteFontMonospaced);

            // Load proprtional font
            //proportionalFontTextBox.Text = SetFontProperties(proportionalFontTextBox, Settings.Default.NoteFontProportional);
        
            // Load background color
            backgroundColorPicker.SelectedColor = Settings.Default.ColorBackground;

            // Load foreground color
            foregroundColorPicker.SelectedColor = Settings.Default.ColorForeground;
        }

        //private string SetFontProperties(DependencyObject control, FontDefinition fontDefinition)
        //{            
            //control.FontFamily = new System.Windows.Media.FontFamily(font.FontFamily.Name);
            //control.FontSize = font.Size;

            //fontDefinition.ApplyToDependencyObject(control);

            //return string.Format("{0}, {1}pt", font.FontFamily.Name, font.SizeInPoints);
        //}

        private void notesDirectoryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // TODO: Implement validation
        }

        private void extensionsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // TODO: Implement validation
        }

        private void proprtionalFontButton_Click(object sender, RoutedEventArgs e)
        {
            ShowFontDialog(proportionalFontTextBox);
        }

        private void monospaceFontButton_Click(object sender, RoutedEventArgs e)
        {
            ShowFontDialog(monospacedFontTextBox);
        }

        private void ShowFontDialog(TextBox textBox)
        {
            var fontChooser = new FontChooser
            {
                Owner = this,
                PreviewSampleText = textBox.SelectedText
            };

            fontChooser.SetPropertiesFromObject(textBox);

            var result = fontChooser.ShowDialog();
            
            if (result != null && result.Value)
            {
                fontChooser.ApplyPropertiesToObject(textBox);
            }
        }
    }
}