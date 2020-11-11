using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using OpenSage.Data;

namespace OpenSage.Tools.AptEditor.Util
{
    internal static class GameFileSystemUtilities
    {
        public static IEnumerable<FileSystemEntry> FindFiles(this FileSystem? fileSystem, Func<FileSystemEntry, bool> matcher)
        {
            if(fileSystem is null)
            {
                return Enumerable.Empty<FileSystemEntry>();
            }
            return fileSystem.Files
                .Where(matcher)
                .Concat(fileSystem.NextFileSystem.FindFiles(matcher));
        }
    }
}
