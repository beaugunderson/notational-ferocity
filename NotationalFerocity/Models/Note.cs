using System;
using System.IO;
using System.Windows;

namespace NotationalFerocity.Models
{
    public class Note : DependencyObject
    {
        public Note(FileSystemInfo fileSystemInfo)
        {
            FileSystemInfo = fileSystemInfo;
        }

        public FileSystemInfo FileSystemInfo { get; set; }

        public string Tags { get; set; }

        public DependencyProperty FileNameProperty =
            DependencyProperty.Register("FileName",
            typeof (string),
            typeof (Note),
            new PropertyMetadata(default(string), FileNameChangedCallback));

        private static void FileNameChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Console.WriteLine("{0} {1}", e.OldValue, e.NewValue);

            var note = d as Note;

            if (note != null)
            {
                File.Move(note.FileSystemInfo.FullName, e.NewValue + note.FileSystemInfo.Extension);
            }
        }

        public string FileName
        {
            get
            {
                return (string)GetValue(FileNameProperty);
            }

            set
            {
                SetValue(FileNameProperty, value);
            }
        }

        public override string ToString()
        {
            return Path.GetFileNameWithoutExtension(FileSystemInfo.Name) ?? "";
        }
    }
}