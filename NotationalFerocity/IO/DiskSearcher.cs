using System.Collections.ObjectModel;
using System.IO;
using System.Threading;

using BeauGunderson.Extensions;

namespace NotationalFerocity.IO
{
    public class DiskSearcher
    {
        public string Directory { get; set; }
        public string SearchTerm { get; set; }

        public CancellationToken CancellationToken { get; set; }

        public Collection<FileInfo> Results { get; set; }

        public event OnCompletion OnCompletionEvent;

        public delegate void OnCompletion(object sender, OnCompletionEventArgs args);
    
        public DiskSearcher(string directory)
        {
            Directory = directory;

            Results = new Collection<FileInfo>();
            CancellationToken = new CancellationToken();
        }

        public void Search(string text)
        {
            SearchTerm = text;

            CancellationToken.ThrowIfCancellationRequested();

            var directory = new DirectoryInfo(Directory);

            if (directory.Exists)
            {
                directory.TraverseTree(HandleFile, HandleDirectory, CancellationToken);
            }

            var handler = OnCompletionEvent;

            if (handler != null)
            {
                handler(this, new OnCompletionEventArgs(Results));
            }
        }

        public void Cancel()
        {
            // CancellationToken
        }

        private bool HandleDirectory(DirectoryInfo file)
        {
            return true;
        }

        private bool HandleFile(FileInfo file)
        {
            if (file.FindInFile(SearchTerm))
            {
                Results.Add(file);
            }

            return true;
        }
    }
}