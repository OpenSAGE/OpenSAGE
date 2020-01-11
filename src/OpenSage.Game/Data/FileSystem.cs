using System;
using System.Collections.Generic;
using System.IO;
using OpenSage.FileFormats.Big;

namespace OpenSage.Data
{
    public sealed class FileSystem : IDisposable
    {
        private readonly Dictionary<string, FileSystemEntry> _fileTable;
        private readonly List<BigArchive> _bigArchives;
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public string RootDirectory { get; }

        public IReadOnlyCollection<FileSystemEntry> Files => _fileTable.Values;
        public FileSystem NextFileSystem { get; }

        public FileSystem(string rootDirectory, FileSystem nextFileSystem = null)
        {
            RootDirectory = rootDirectory;

            NextFileSystem = nextFileSystem;

            _fileTable = new Dictionary<string, FileSystemEntry>();
            _bigArchives = new List<BigArchive>();

            // First create entries for all non-.big files
            if (Directory.Exists(rootDirectory))
            {
                foreach (var file in Directory.GetFiles(rootDirectory, "*.*", SearchOption.AllDirectories))
                {
                    var ext = Path.GetExtension(file).ToLowerInvariant();
                    if (ext != ".big")
                    {
                        var relativePath = file.Substring(rootDirectory.Length);
                        if (relativePath.StartsWith(Path.DirectorySeparatorChar.ToString()))
                        {
                            relativePath = relativePath.Substring(1);
                        }
                        relativePath = NormalizeFilePath(relativePath);
                        _fileTable.Add(relativePath, new FileSystemEntry(this, relativePath, (uint) new FileInfo(file).Length, () => File.OpenRead(file)));
                    }
                }

                // Then load .big files
                SkudefReader.Read(rootDirectory, path => AddBigArchive(path));
            }
            else
            {
                logger.Warn("Failed to create filesystem for non existing root directory: " + rootDirectory);
            }
        }

        private void AddBigArchive(string path)
        {
            var archive = new BigArchive(path);

            _bigArchives.Add(archive);

            foreach (var entry in archive.Entries)
            {
                var filePath = NormalizeFilePath(entry.FullName);
                if (!_fileTable.ContainsKey(filePath))
                {
                    _fileTable.Add(filePath, new FileSystemEntry(this, filePath, entry.Length, entry.Open));
                }
            }
        }

        public static string NormalizeFilePath(string filePath)
        {
            return filePath
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar)
                .ToLowerInvariant();
        }

        public FileSystemEntry GetFile(string filePath)
        {
            filePath = NormalizeFilePath(filePath);

            if (_fileTable.TryGetValue(filePath, out var file))
            {
                return file;
            }

            return NextFileSystem?.GetFile(filePath);
        }

        public FileSystemEntry SearchFile(string fileName, params string[] searchFolders)
        {
            fileName = NormalizeFilePath(fileName);

            foreach (var searchFolder in searchFolders)
            {
                var normalizedSearchFolder = NormalizeFilePath(searchFolder);
                if (_fileTable.TryGetValue(Path.Combine(searchFolder, fileName), out var file))
                {
                    return file;
                }
            }

            return NextFileSystem?.SearchFile(fileName, searchFolders);
        }

        public IEnumerable<FileSystemEntry> GetFiles(string folderPath)
        {
            folderPath = NormalizeFilePath(folderPath);

            foreach (var entry in _fileTable.Values)
            {
                if (entry.FilePath.StartsWith(folderPath, StringComparison.InvariantCultureIgnoreCase))
                {
                    yield return entry;
                }
            }

            if (NextFileSystem != null)
            {
                foreach (var entry in NextFileSystem.GetFiles(folderPath))
                {
                    yield return entry;
                }
            }
        }

        public void Dispose()
        {
            foreach (var archive in _bigArchives)
            {
                archive.Dispose();
            }
            _bigArchives.Clear();
        }
    }
}
