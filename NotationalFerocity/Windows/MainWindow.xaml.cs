using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

using NotationalFerocity.Models;
using NotationalFerocity.Properties;
using NotationalFerocity.Utilities;

namespace NotationalFerocity.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private DeferredAction _deferredAction;
        private readonly TimeSpan _saveDelay = TimeSpan.FromMilliseconds(500);
        private Note _currentNote;

        public ObservableCollection<Note> Notes { get; private set; }
        public StringCollection Extensions { get; private set; }
        public FileSystemWatcher NoteWatcher { get; private set; }

        protected bool LoadingText { get; set; }

        public Note CurrentNote { 
            get
            {
                return _currentNote;
            }
            
            private set
            {
                _currentNote = value;

                notesListBox.SelectedItem = _currentNote;
            }
        }
        
        public MainWindow()
        {
            InitializeComponent();

            Notes = new ObservableCollection<Note>();

            GetView().CustomSort = new AlphanumComparator();

            try
            {
                Extensions = (StringCollection)Settings.Default["Extensions"];

                if (!Directory.Exists(Settings.Default.NotesDirectory))
                {
                    var result = MessageBox.Show(
                        string.Format("The directory you've specified for your notes database doesn't exist. Would you like to create it?\n\n{0}",
                            Settings.Default.NotesDirectory),
                        "Notes directory not found",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        Directory.CreateDirectory(Settings.Default.NotesDirectory);
                    }
                    else
                    {
                        new SettingsWindow();
                    }
                }

                NoteWatcher = new FileSystemWatcher(Settings.Default.NotesDirectory);
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("There was an error loading your saved settings: {0}", e.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                Application.Current.Shutdown();
            }

            NoteWatcher.Renamed += NoteWatcher_Renamed;

            NoteWatcher.Created += NoteWatcher_Modified;
            NoteWatcher.Deleted += NoteWatcher_Modified;

            NoteWatcher.IncludeSubdirectories = true;
            NoteWatcher.EnableRaisingEvents = false;

            DataContext = this;
        }

        private void InvokeIfNeeded(Action action)
        {
            if (Dispatcher.Thread != Thread.CurrentThread)
            {
                Dispatcher.Invoke(action);
            }
            else
            {
                action();
            }
        }

        void NoteWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            Console.WriteLine("File renamed: {0} -> {1}", e.OldName, e.Name);

            InvokeIfNeeded(RefreshNotes);
        }
        
        void NoteWatcher_Modified(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("File {1}: {0}", e.Name, e.ChangeType.ToString().ToLower());

            InvokeIfNeeded(RefreshNotes);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InvokeIfNeeded(RefreshNotes);
        }

        private ListCollectionView GetView()
        {
            return (ListCollectionView)CollectionViewSource.GetDefaultView(Notes);
        }

        private void RefreshNotes()
        {
            Notes.Clear();

            foreach (var note in GetNotes())
            {
                Notes.Add(new Note(note));
            }
        }

        private IEnumerable<FileSystemInfo> GetNotes()
        {
            var queue = new Queue<DirectoryInfo>();

            queue.Enqueue(new DirectoryInfo(Settings.Default.NotesDirectory));

            while (queue.Count > 0)
            {
                var path = queue.Dequeue();

                foreach (var directory in path.GetDirectories())
                {
                    queue.Enqueue(directory);
                }

                var files = path.GetFileSystemInfos();

                foreach (var file in files)
                {
                    if (file.Attributes.HasFlag(FileAttributes.Directory))
                    {
                        continue;
                    }

                    if (!Extensions.Contains(file.Extension.ToLower()))
                    {
                        continue;
                    }

                    Console.WriteLine(file.FullName);

                    yield return file;
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.Default.Save();
        }

        private void notesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1)
            {
                return;
            }

            CurrentNote = (Note)e.AddedItems[0];

            UpdateText();
        }

        private void UpdateText()
        {
            LoadingText = true;

            noteRichTextBox.Document.Blocks.Clear();

            var paragraph = new Paragraph();
            
            paragraph.Inlines.Add(new Run(File.ReadAllText(CurrentNote.FileSystemInfo.FullName)));

            noteRichTextBox.Document.Blocks.Add(paragraph);

            LoadingText = false;
        }

        /// <summary>
        /// Filter based on whether the note contains the text in searchTextBox.Text
        /// </summary>
        private bool SearchFilter(object o)
        {
            return o as Note != null && (o as Note).ToString().ToLower().Contains(searchTextBox.Text.ToLower());
        }

        /// <summary>
        /// Applies a filter to the list of notes each time the user types
        /// into the search field. If there's only one item returned we
        /// select it and display the text.
        /// </summary>
        private void searchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(searchTextBox.Text))
            {
                GetView().Filter = null;
            }
            else
            {
                GetView().Filter = SearchFilter;
            }

            if (notesListBox.Items.Count == 1)
            {
                CurrentNote = GetView().GetItemAt(0) as Note;

                UpdateText();
            }
        }

        /// <summary>
        /// A convenience function to get the contents of the RichTextBox
        /// </summary>
        /// <returns>The contents of the RichTextBox</returns>
        private string GetText()
        {
            return new TextRange(noteRichTextBox.Document.ContentStart,
                                 noteRichTextBox.Document.ContentEnd).Text;
        }

        /// <summary>
        /// Only save once the user has stopped typing.
        /// </summary>
        private void noteRichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (LoadingText)
            {
                return;
            }

            if (_deferredAction == null)
            {
                _deferredAction = DeferredAction.Create(SaveText);
            }

            _deferredAction.Defer(_saveDelay);
        }

        /// <summary>
        /// Save the contents of the RichTextBox to the note file.
        /// </summary>
        private void SaveText()
        {
            File.WriteAllText(CurrentNote.FileSystemInfo.FullName, GetText());
        }

        /// <summary>
        /// Focus an existing note or create a new note when the user hits enter.
        /// </summary>
        private void searchTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Enter)
            {
                return;
            }

            foreach (var note in Notes.Where(
                note => note.FileNameWithoutExtension.ToLower() == searchTextBox.Text.ToLower()))
            {
                CurrentNote = note;

                return;
            }

            var newNote = Note.FromTitle(searchTextBox.Text);

            Notes.Add(newNote);

            CurrentNote = newNote;
        }
    }
}