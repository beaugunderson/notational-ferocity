using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;

using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using MenuItem = System.Windows.Controls.MenuItem;

using NotationalFerocity.Models;
using NotationalFerocity.Properties;
using NotationalFerocity.Utilities;
using NotationalFerocity.WPF;

namespace NotationalFerocity.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private DeferredAction _deferredAction;
        private WindowState _storedWindowState;

        private readonly TimeSpan _saveDelay = TimeSpan.FromMilliseconds(500);
        private readonly NotifyIcon _notifyIcon;

        public ObservableCollection<Note> Notes { get; private set; }
        public StringCollection Extensions { get; private set; }
        public FileSystemWatcher NoteWatcher { get; private set; }

        protected bool LoadingText { get; set; }

        private const uint SettingsMenuId = 1000;
        private const uint AboutMenuId = 1001;
        private const uint MonospacedId = 1002;

        internal override bool HandleWndProc(IntPtr wParam)
        {
            // Execute the appropriate code for the System Menu item that was clicked
            switch (Convert.ToUInt32(wParam.ToInt32()))
            {
                case SettingsMenuId:
                    var settings = new SettingsWindow();

                    settings.ShowDialog();

                    return true;
                case AboutMenuId:
                    MessageBox.Show("A work in progress.");

                    return true;
                case MonospacedId:
                    switch (noteRichTextBox.FontSelection)
                    {
                        case FontSelection.Proportional:
                            noteRichTextBox.FontSelection = FontSelection.Monospaced;

                            ModifyMenu(SystemMenuHandle, 9, MF_BYPOSITION | MF_CHECKED, MonospacedId, "Use &monospaced font");

                            break;
                        default:
                            noteRichTextBox.FontSelection = FontSelection.Proportional;

                            ModifyMenu(SystemMenuHandle, 9, MF_BYPOSITION | MF_UNCHECKED, MonospacedId, "Use &monospaced font");

                            break;
                    }

                    return true;
            }

            return false;
        }

        public static readonly DependencyProperty CurrentNoteProperty =
            DependencyProperty.Register("CurrentNote",
                typeof(Note),
                typeof(MainWindow),
                new PropertyMetadata(default(Note)));

        public Note CurrentNote
        {
            get
            {
                return (Note)GetValue(CurrentNoteProperty);
            }

            set
            {
                SetValue(CurrentNoteProperty, value);
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


            var iconInfo = Application.GetResourceStream(new Uri(@"pack://application:,,/Images/MainIcon.ico"));

            if (iconInfo == null)
            {
                throw new ApplicationException("Unable to load the application icon.");
            }

            using (iconInfo.Stream)
            {
                _notifyIcon = new NotifyIcon
                {
                    BalloonTipText = "Notational Ferocity has been minimized. Click the tray icon to show it.",
                    BalloonTipTitle = "Notational Ferocity",
                    Text = "Notational Ferocity",
                    Icon = new Icon(iconInfo.Stream)
                };

                _notifyIcon.Click += notifyIcon_Click;
            }

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
            // Create our new System Menu items just before the Close menu item
            InsertMenu(SystemMenuHandle, 5, MF_BYPOSITION | MF_SEPARATOR, 0, string.Empty);

            InsertMenu(SystemMenuHandle, 6, MF_BYPOSITION, SettingsMenuId, "&Settings...");
            InsertMenu(SystemMenuHandle, 7, MF_BYPOSITION, AboutMenuId, "&About...");

            InsertMenu(SystemMenuHandle, 8, MF_BYPOSITION | MF_SEPARATOR, 0, string.Empty);

            InsertMenu(SystemMenuHandle, 9, MF_BYPOSITION, MonospacedId, "Use &monospaced font");

            RefreshNotes();
        }

        private ListCollectionView GetView()
        {
            return (ListCollectionView)CollectionViewSource.GetDefaultView(Notes);
        }

        /// <summary>
        /// A generator that returns notes from the filesystem.
        /// </summary>
        /// <returns></returns>
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

                foreach (var file in files
                    .Where(file => !file.Attributes.HasFlag(FileAttributes.Directory))
                    .Where(file => Extensions.Contains(file.Extension.ToLower())))
                {
                    Console.WriteLine(file.FullName);

                    yield return file;
                }
            }
        }

        private void RefreshNotes()
        {
            Notes.Clear();

            foreach (var note in GetNotes())
            {
                Notes.Add(new Note(note));
            }
        }

        private void AddNote(Note note)
        {
            Notes.Add(note);

            CurrentNote = note;

            noteRichTextBox.Focus();
        }

        private void UpdateText()
        {
            LoadingText = true;

            noteRichTextBox.Document = CurrentNote.Document;

            LoadingText = false;
        }

        /// <summary>
        /// Filter based on whether the note contains the text in searchTextBox.Text
        /// </summary>
        private bool SearchFilter(object item)
        {
            return item as Note != null &&
                (item as Note).ToString().ToLower().Contains(searchTextBox.Text.ToLower());
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

            if (notesListView.Items.Count == 1)
            {
                CurrentNote = GetView().GetItemAt(0) as Note;
            }
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

            // TODO: Make this use binding instead?
            CurrentNote.Text = noteRichTextBox.GetText();

            if (_deferredAction == null)
            {
                _deferredAction = DeferredAction.Create(CurrentNote.Save);
            }

            _deferredAction.Defer(_saveDelay);
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

            AddNote(Note.FromTitle(searchTextBox.Text));
        }

        private void Markdown_Click(object sender, RoutedEventArgs e)
        {
            var markdownWindow = new MarkdownWindow(CurrentNote);

            markdownWindow.Show();
        }

        private void Rename_Click(object sender, RoutedEventArgs e)
        {

        }

        private void InteropWindow_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();

                if (_notifyIcon != null)
                {
                    _notifyIcon.ShowBalloonTip(2000);
                }
            }
            else
            {
                _storedWindowState = WindowState;
            }
        }

        private void InteropWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = !IsVisible;
            }
        }

        void notifyIcon_Click(object sender, EventArgs e)
        {
            WindowState = _storedWindowState;

            Show();
        }

        private void InteropWindow_Closed(object sender, EventArgs eventArgs)
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Dispose();
            }
        }

        /// <summary>
        /// Add spelling suggestions to the context menu
        /// </summary>
        private void noteRichTextBox_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            // Clear the context menu from its previous suggestions.
            noteContextMenu.Items.Clear();

            int cmdIndex = 0;

            var markdownMenuItem = new MenuItem
            {
                Header = "Markdown"
            };

            markdownMenuItem.Click += Markdown_Click;

            noteRichTextBox.ContextMenu.Items.Insert(cmdIndex++, markdownMenuItem);
            
            var spellingError = noteRichTextBox.GetSpellingError(noteRichTextBox.CaretPosition);

            if (spellingError == null)
            {
                return;
            }

            noteRichTextBox.ContextMenu.Items.Insert(cmdIndex++, new Separator());

            int count = 0;

            // Add each suggestion up to 5 total
            foreach (var str in spellingError.Suggestions)
            {
                if (count > 4)
                {
                    break;
                }

                var mi = new MenuItem
                {
                    Header = str,
                    FontWeight = FontWeights.Bold,
                    Command = EditingCommands.CorrectSpellingError,
                    CommandParameter = str,
                    CommandTarget = noteRichTextBox
                };

                noteRichTextBox.ContextMenu.Items.Insert(cmdIndex++, mi);
                    
                count++;
            }
            
            // Add separator lines and IgnoreAll command.
            noteRichTextBox.ContextMenu.Items.Insert(cmdIndex++, new Separator());
                
            var ignoreAllMenuItem = new MenuItem
            {
                Header = "Ignore All",
                Command = EditingCommands.IgnoreSpellingError,
                CommandTarget = noteRichTextBox
            };

            noteRichTextBox.ContextMenu.Items.Insert(cmdIndex, ignoreAllMenuItem);
        }
    }
}