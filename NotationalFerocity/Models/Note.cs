using System;
using System.IO;
using System.Web;
using System.Windows;
using System.Windows.Documents;

using NotationalFerocity.Utilities;

namespace NotationalFerocity.Models
{
    public class Note : DependencyObject
    {
        private string _text;

        public Note(FileSystemInfo fileSystemInfo)
        {
            FileSystemInfo = fileSystemInfo;
        }

        public FileSystemInfo FileSystemInfo { get; set; }

        public string Tags { get; set; }

        public string FileNameWithoutExtension
        {
            get
            {
                return HttpUtility.UrlDecode(Path.GetFileNameWithoutExtension(FileSystemInfo.FullName));
            }

            set
            {
                var directory = Path.GetDirectoryName(FileSystemInfo.FullName);

                if (directory == null)
                {
                    throw new ApplicationException(string.Format("Unable to ascertain the directory for note {0}", this));
                }

                var destination = Path.Combine(directory,
                    HttpUtility.UrlPathEncode(value + FileSystemInfo.Extension)).Replace("%20", " ");

                Console.WriteLine("Name changed, renaming file from {0} to {1}",
                    FileSystemInfo.FullName, destination);

                File.Move(FileSystemInfo.FullName, destination);

                FileSystemInfo = new FileInfo(destination);
            }
        }

        public override string ToString()
        {
            return FileNameWithoutExtension;
        }

        public static Note FromTitle(string title)
        {
            var destination = Paths.CombineNotesDirectory(title + ".txt");

            File.CreateText(destination).Close();

            return new Note(new FileInfo(destination));
        }

        public void Save()
        {
            Console.WriteLine("Saving...");

            File.WriteAllText(FileSystemInfo.FullName, Text);
        }

        public FlowDocument Document
        {
            get
            {
                return new FlowDocument(new Paragraph(new Run(Text)));
            }
        }

        public string Text
        {
            // Lazy-load the contents of the file
            get
            {
                // XXX: Add exception-handling here (out of memory on large files)
                return _text ?? (_text = File.ReadAllText(FileSystemInfo.FullName));
            }

            set
            {
                _text = value;
            }
        }
    }
}