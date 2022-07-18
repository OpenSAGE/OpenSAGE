using System.Collections.Generic;
using System.IO;

namespace OpenSage.IO
{
    public sealed class CompositeFileSystem : FileSystem
    {
        private readonly FileSystem[] _fileSystems;

        public CompositeFileSystem(params FileSystem[] fileSystems)
        {
            _fileSystems = fileSystems;

            foreach (var fileSystem in _fileSystems)
            {
                AddDisposable(fileSystem);
            }
        }

        public override FileSystemEntry GetFile(string filePath)
        {
            foreach (var fileSystem in _fileSystems)
            {
                var fileSystemEntry = fileSystem.GetFile(filePath);
                if (fileSystemEntry != null)
                {
                    return fileSystemEntry;
                }
            }

            return null;
        }

        public override IEnumerable<FileSystemEntry> GetFilesInDirectory(
            string directoryPath,
            string searchPattern,
            SearchOption searchOption)
        {
            var paths = new HashSet<string>();

            foreach (var fileSystem in _fileSystems)
            {
                foreach (var fileSystemEntry in fileSystem.GetFilesInDirectory(directoryPath, searchPattern, searchOption))
                {
                    if (paths.Contains(fileSystemEntry.FilePath))
                    {
                        continue;
                    }

                    paths.Add(fileSystemEntry.FilePath);

                    yield return fileSystemEntry;
                }
            }
        }
    }
}
