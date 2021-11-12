using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenSage.Data;
using OpenSage.FileFormats.Big;

namespace OpenSage.IO
{
    public sealed class BigFileSystem : FileSystem
    {
        private readonly BigDirectory _rootDirectory;

        public BigFileSystem(string rootDirectory)
        {
            _rootDirectory = new BigDirectory();

            SkudefReader.Read(rootDirectory, AddBigArchive);
        }

        private void AddBigArchive(string path)
        {
            var bigArchive = AddDisposable(new BigArchive(path));

            foreach (var bigArchiveEntry in bigArchive.Entries)
            {
                var directoryParts = bigArchiveEntry.FullName.Split('\\', '/');

                var bigDirectory = _rootDirectory;
                for (var i = 0; i < directoryParts.Length - 1; i++)
                {
                    bigDirectory = bigDirectory.GetOrCreateDirectory(directoryParts[i]);
                }

                var fileName = directoryParts[directoryParts.Length - 1];

                bigDirectory.Files.TryAdd(fileName, bigArchiveEntry);
            }
        }

        public override IEnumerable<FileSystemEntry> GetFilesInDirectory(
            string directoryPath,
            string searchPattern,
            SearchOption searchOption)
        {
            var search = new SearchPattern(searchPattern);

            var directoryParts = directoryPath.Split(Path.DirectorySeparatorChar);

            var bigDirectory = _rootDirectory;
            for (var i = 0; i < directoryParts.Length; i++)
            {
                if (!bigDirectory.Directories.TryGetValue(directoryParts[i], out bigDirectory))
                {
                    return Enumerable.Empty<FileSystemEntry>();
                }
            }

            return GetFilesInDirectory(bigDirectory, search, searchOption);
        }

        private IEnumerable<FileSystemEntry> GetFilesInDirectory(
            BigDirectory bigDirectory,
            SearchPattern searchPattern,
            SearchOption searchOption)
        {
            foreach (var file in bigDirectory.Files.Values)
            {
                if (!searchPattern.Match(file.FullName))
                {
                    continue;
                }

                yield return CreateFileSystemEntry(file);
            }

            if (searchOption != SearchOption.AllDirectories)
            {
                foreach (var directory in bigDirectory.Directories.Values)
                {
                    foreach (var file in GetFilesInDirectory(directory, searchPattern, searchOption))
                    {
                        yield return file;
                    }
                }
            }
        }

        public override FileSystemEntry GetFile(string filePath)
        {
            var directoryParts = filePath.Split(Path.DirectorySeparatorChar);

            var bigDirectory = _rootDirectory;
            for (var i = 0; i < directoryParts.Length - 1; i++)
            {
                if (!bigDirectory.Directories.TryGetValue(directoryParts[i], out bigDirectory))
                {
                    return null;
                }
            }

            var fileName = directoryParts[directoryParts.Length - 1];

            if (!bigDirectory.Files.TryGetValue(fileName, out var file))
            {
                return null;
            }

            return CreateFileSystemEntry(file);
        }

        private FileSystemEntry CreateFileSystemEntry(BigArchiveEntry entry)
        {
            return new FileSystemEntry(
                this,
                NormalizeFilePath(entry.FullName),
                entry.Length,
                entry.Open);
        }

        private sealed class BigDirectory
        {
            public readonly Dictionary<string, BigDirectory> Directories = new Dictionary<string, BigDirectory>(StringComparer.InvariantCultureIgnoreCase);
            public readonly Dictionary<string, BigArchiveEntry> Files = new Dictionary<string, BigArchiveEntry>(StringComparer.InvariantCultureIgnoreCase);

            public BigDirectory GetOrCreateDirectory(string directoryName)
            {
                if (!Directories.TryGetValue(directoryName, out var directory))
                {
                    directory = new BigDirectory();
                    Directories.Add(directoryName, directory);
                }
                return directory;
            }
        }
    }
}
