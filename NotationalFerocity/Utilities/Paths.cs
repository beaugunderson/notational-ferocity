using System;
using System.IO;
using System.Web;
using NotationalFerocity.Properties;

namespace NotationalFerocity.Utilities
{
    public static class Paths
    {
        public static string CombineBaseDirectory(string path)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
        }

        public static string CombineNotesDirectory(string path)
        {
            return Path.Combine(Settings.Default.NotesDirectory, HttpUtility.UrlPathEncode(path));
        }
    }
}
