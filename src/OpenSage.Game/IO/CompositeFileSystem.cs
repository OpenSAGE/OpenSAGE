using System.Collections.Generic;

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
            bool includeSubdirectories)
        {
            foreach (var fileSystem in _fileSystems)
            {
                foreach (var fileSystemEntry in fileSystem.GetFilesInDirectory(directoryPath, searchPattern, includeSubdirectories))
                {
                    yield return fileSystemEntry;
                }
            }
        }
    }
}
