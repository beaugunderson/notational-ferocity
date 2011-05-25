using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
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
        private string _notesDirectory;
        private string _extensions;

        public SettingsWindow()
        {
            InitializeComponent();

            DataContext = this;

            // Load notes directory
            NotesDirectory = Settings.Default.NotesDirectory;

            // Load file extensions
            var extensions = new string[Settings.Default.Extensions.Count];

            Settings.Default.Extensions.CopyTo(extensions, 0);

            Extensions = String.Join("; ", extensions);
        }

        public string NotesDirectory
        {
            get
            {
                return _notesDirectory;
            }

            set
            {
                _notesDirectory = value;
                
                if (!Directory.Exists(value))
                {
                    throw new ApplicationException("The directory does not exist.");
                }
            }
        }

        public string Extensions
        {
            get {
                return _extensions;
            }

            set
            {
                _extensions = value;

                if (String.IsNullOrWhiteSpace(value))
                {
                    throw new ApplicationException("You must provide a list of extensions.");
                }
            }
        }

        /// <summary>
        /// Shows a folder browser so the user can select a notes directory.
        /// </summary>
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
            Settings.Default.FontProportional = SettingsFontDefinition.FromFontDefinition(FontDefinition.FromDependencyObject(proportionalFontTextBox));
            Settings.Default.FontMonospaced = SettingsFontDefinition.FromFontDefinition(FontDefinition.FromDependencyObject(monospacedFontTextBox));
            Settings.Default.FontMarkdown = SettingsFontDefinition.FromFontDefinition(FontDefinition.FromDependencyObject(markdownFontTextBox));
            Settings.Default.FontDisplay = SettingsFontDefinition.FromFontDefinition(FontDefinition.FromDependencyObject(displayFontTextBox));

            Settings.Default.NotesDirectory = notesDirectoryTextBox.Text.Trim();

            var extensions = new StringCollection();

            foreach (var extension in Regex.Split(extensionsTextBox.Text.Trim(), @"[\s,;]+")
                .Where(extension => !String.IsNullOrWhiteSpace(extension)))
            {
                extensions.Add(extension.Trim());
            }

            Settings.Default.Extensions = extensions;

            Settings.Default.ColorBackground = new SolidColorBrush(backgroundColorPicker.SelectedColor);
            Settings.Default.ColorForeground = new SolidColorBrush(foregroundColorPicker.SelectedColor);

            Settings.Default.Save();
            
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Load monospaced font
            monospacedFontTextBox.Text = SetFontProperties(monospacedFontTextBox, Settings.Default.FontMonospaced);

            // Load proprtional font
            proportionalFontTextBox.Text = SetFontProperties(proportionalFontTextBox, Settings.Default.FontProportional);
        
            // Load background color
            backgroundColorPicker.SelectedColor = Settings.Default.ColorBackground.Color;

            // Load foreground color
            foregroundColorPicker.SelectedColor = Settings.Default.ColorForeground.Color;
        }

        private string SetFontProperties(DependencyObject control, SettingsFontDefinition settingsFontDefinition)
        {
            var fontDefinition = settingsFontDefinition.ToFontDefinition();
            
            fontDefinition.ApplyToDependencyObject(control);

            return string.Format("{0}, {1}pt", fontDefinition.FontFamily, fontDefinition.FontSize);
        }

        private void proportionalFontButton_Click(object sender, RoutedEventArgs e)
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

            if (result == null || !result.Value)
            {
                return;
            }

            var fontDefinition = new FontDefinition();

            fontChooser.ApplyPropertiesToObject(textBox);
            fontChooser.ApplyPropertiesToObject(fontDefinition);

            textBox.Text = fontDefinition.ToString();
        }
    }
}