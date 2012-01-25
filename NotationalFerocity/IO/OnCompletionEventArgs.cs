using System.Collections.ObjectModel;
using System.IO;

namespace NotationalFerocity.IO
{
    public class OnCompletionEventArgs
    {
        public Collection<FileInfo> Results { get; set; }

        public OnCompletionEventArgs(Collection<FileInfo> results)
        {
            Results = results;
        }
    }
}