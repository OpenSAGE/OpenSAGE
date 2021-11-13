using System.Collections.Generic;
using System.IO;

namespace OpenSage.IO
{
    public abstract class FileSystem : DisposableBase
    {
        public static string NormalizeFilePath(string filePath)
        {
            return filePath
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);
        }

        public abstract FileSystemEntry GetFile(string filePath);

        public abstract IEnumerable<FileSystemEntry> GetFilesInDirectory(
            string directoryPath,
            string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly);
    }
}
