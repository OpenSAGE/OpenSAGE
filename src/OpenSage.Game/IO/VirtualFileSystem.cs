using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenSage.IO
{
    public sealed class VirtualFileSystem : FileSystem
    {
        private readonly string _virtualDirectory;
        private readonly FileSystem _targetFileSystem;

        public VirtualFileSystem(string virtualDirectory, FileSystem targetFileSystem)
        {
            _virtualDirectory = virtualDirectory;
            _targetFileSystem = targetFileSystem;
        }

        public override FileSystemEntry GetFile(string filePath)
        {
            if (!TryGetRelativePath(filePath, out var relativePath))
            {
                return null;
            }

            return _targetFileSystem.GetFile(relativePath);
        }

        public override IEnumerable<FileSystemEntry> GetFilesInDirectory(
            string directoryPath,
            string searchPattern,
            SearchOption searchOption)
        {
            if (!TryGetRelativePath(directoryPath, out var relativePath))
            {
                return Enumerable.Empty<FileSystemEntry>();
            }

            return _targetFileSystem.GetFilesInDirectory(
                relativePath,
                searchPattern,
                searchOption);
        }

        private bool TryGetRelativePath(string path, out string relativePath)
        {
            if (!path.StartsWith(_virtualDirectory))
            {
                relativePath = null;
                return false;
            }

            relativePath = path.Substring(_virtualDirectory.Length);
            return true;
        }
    }
}
