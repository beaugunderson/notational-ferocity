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

namespace NotationalFerocity
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public ObservableCollection<Note> Notes { get; private set; }
        public StringCollection Extensions { get; private set; }
        public Note CurrentNote { get; private set; }
        public FileSystemWatcher NoteWatcher { get; private set; }

        public MainWindow()
        {
            InitializeComponent();

            Notes = new ObservableCollection<Note>();

            try
            {
                Extensions = (StringCollection)Settings.Default["Extensions"];

                if (!Directory.Exists(Settings.Default.NotesDirectory))
                {
                    // Show Settings
                    var result = MessageBox.Show(
                        "The directory you've specified for your notes database doesn't exist. Would you like to create it?\n\n" +
                        Settings.Default.NotesDirectory, "Notes directory not found", MessageBoxButton.YesNo, MessageBoxImage.Question);

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
            NoteWatcher.Changed += NoteWatcher_Modified;

            NoteWatcher.IncludeSubdirectories = true;
            NoteWatcher.EnableRaisingEvents = true;

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
            GetView().CustomSort = new AlphanumComparator();

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

        protected bool LoadingText
        {
            get;
            set;
        }

        private bool SearchFilter(object o)
        {
            var note = o as Note;

            return note != null && note.ToString().ToLower().Contains(searchTextBox.Text.ToLower());
        }

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

            //Console.WriteLine("{0} - {1} - {2} - {3}", 
            //    notesListBox.Items.Count,
            //    GetView().Cast<Note>().Count(),
            //    GetView().Count,
            //    Notes.Count);

            if (notesListBox.Items.Count == 1)
            {
                CurrentNote = GetView().GetItemAt(0) as Note;

                UpdateText();
            }
        }

        private string GetText()
        {
            return new TextRange(noteRichTextBox.Document.ContentStart,
                                 noteRichTextBox.Document.ContentEnd).Text;
        }

        private void noteRichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (LoadingText)
            {
                return;
            }

            File.WriteAllText(CurrentNote.FileSystemInfo.FullName, GetText());
        }

        private void notesListBox_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            Console.WriteLine("Source updated.");
        }
    }
}