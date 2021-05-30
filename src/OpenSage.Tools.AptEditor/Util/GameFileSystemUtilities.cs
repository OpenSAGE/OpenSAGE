using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.IO;

namespace OpenSage.Tools.AptEditor.Util
{
    internal static class GameFileSystemUtilities
    {
        public static IEnumerable<FileSystemEntry> FindFiles(this FileSystem fileSystem, Func<FileSystemEntry, bool> matcher)
        {
            return fileSystem
                .GetFilesInDirectory("", "*", true)
                .Where(matcher);
        }
    }
}
