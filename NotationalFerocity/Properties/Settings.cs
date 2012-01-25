using System;

namespace NotationalFerocity.Properties
{
    public sealed partial class Settings
    {
        public string ExpandedNotesDirectory
        {
            get
            {
                return Environment.ExpandEnvironmentVariables(NotesDirectory);
            }
        }
    }
}