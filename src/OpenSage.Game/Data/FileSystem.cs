using System;
using System.Collections.Generic;
using System.IO;
using OpenSage.Data.Big;

namespace OpenSage.Data
{
    public sealed class FileSystem : IDisposable
    {
        private readonly FileSystem _nextFileSystem;

        private readonly Dictionary<string, FileSystemEntry> _fileTable;
        private readonly List<BigArchive> _bigArchives;

        public string RootDirectory { get; }

        public IReadOnlyCollection<FileSystemEntry> Files => _fileTable.Values;
        public FileSystem NextFileSystem => _nextFileSystem;

        public FileSystem(string rootDirectory, FileSystem nextFileSystem = null)
        {
            RootDirectory = rootDirectory;

            _nextFileSystem = nextFileSystem;

            _fileTable = new Dictionary<string, FileSystemEntry>(StringComparer.OrdinalIgnoreCase);
            _bigArchives = new List<BigArchive>();

            // TODO: Figure out if there's a specific order that .big files should be loaded in,
            // since some files are contained in more than one .big file so the later one
            // takes precedence over the earlier one.

            foreach (var file in Directory.GetFiles(rootDirectory, "*.*", SearchOption.AllDirectories))
            {
                var ext = Path.GetExtension(file).ToLowerInvariant();
                if (ext == ".big")
                {
                    var archive = new BigArchive(file);

                    _bigArchives.Add(archive);

                    foreach (var entry in archive.Entries)
                    {
                        var filePath = NormalizeFilePath(entry.FullName);
                        _fileTable[filePath] = new FileSystemEntry(this, NormalizeFilePath(filePath), entry.Length, entry.Open);
                    }
                }
                else
                {
                    var relativePath = file.Substring(rootDirectory.Length);
                    if (relativePath.StartsWith(Path.DirectorySeparatorChar.ToString()))
                    {
                        relativePath = relativePath.Substring(1);
                    }
                    _fileTable[relativePath] = new FileSystemEntry(this, NormalizeFilePath(relativePath), (uint) new FileInfo(file).Length, () => File.OpenRead(file));
                }
            }
        }

        private static string NormalizeFilePath(string filePath)
        {
            return filePath
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);
        }

        public FileSystemEntry GetFile(string filePath)
        {
            filePath = NormalizeFilePath(filePath);

            if (_fileTable.TryGetValue(filePath, out var file))
            {
                return file;
            }

            return _nextFileSystem?.GetFile(filePath);
        }

        public FileSystemEntry SearchFile(string fileName, params string[] searchFolders)
        {
            fileName = NormalizeFilePath(fileName);

            foreach (var searchFolder in searchFolders)
            {
                if (_fileTable.TryGetValue(Path.Combine(searchFolder, fileName), out var file))
                {
                    return file;
                }
            }

            return _nextFileSystem?.SearchFile(fileName, searchFolders);
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

            if (_nextFileSystem != null)
            {
                foreach (var entry in _nextFileSystem.GetFiles(folderPath))
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
