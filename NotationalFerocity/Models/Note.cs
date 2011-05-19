using System;
using System.IO;
using System.Web;
using System.Windows;

using NotationalFerocity.Properties;

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

        public string FileNameWithoutExtension
        {
            get
            {
                return HttpUtility.UrlDecode(Path.GetFileNameWithoutExtension(FileSystemInfo.FullName));
            }

            set
            {
                var directory = Path.GetDirectoryName(HttpUtility.UrlEncode(FileNameWithoutExtension));

                if (directory == null)
                {
                    throw new ApplicationException(string.Format("Unable to ascertain the directory for note {0}", this));
                }

                var destination = Path.Combine(directory, HttpUtility.UrlEncode(value + FileSystemInfo.Extension));

                Console.WriteLine("Name changed, renaming file from {0} to {1}",
                    FileNameWithoutExtension, destination);

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
            var destination = Path.Combine(Settings.Default.NotesDirectory, HttpUtility.UrlEncode(title + ".txt"));

            var file = File.CreateText(destination);

            file.Close();

            return new Note(new FileInfo(destination));
        }
    }
}