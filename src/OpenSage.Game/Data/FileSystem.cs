using System;
using System.Collections.Generic;
using System.IO;
using OpenSage.FileFormats.Big;

namespace OpenSage.Data
{
    public sealed class FileSystem : IDisposable
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly Dictionary<string, FileSystemEntry> _fileTable;
        private readonly List<BigArchive> _bigArchives;
        private readonly Dictionary<string, string> _realPathsToVirtualPaths;
        
        public string RootDirectory { get; }
        public IReadOnlyCollection<FileSystemEntry> Files => _fileTable.Values;
        public FileSystem NextFileSystem { get; }

        public FileSystem(string rootDirectory,
                          FileSystem nextFileSystem = null,
                          IEnumerable<KeyValuePair<string, string>> realPathsToVirtualPaths = null)
        {
            RootDirectory = rootDirectory;

            NextFileSystem = nextFileSystem;

            _fileTable = new Dictionary<string, FileSystemEntry>();
            _bigArchives = new List<BigArchive>();
            _realPathsToVirtualPaths = new Dictionary<string, string>();
            // Create mapping for virtual paths
            // e.g. C:\Users\lanyi\AppData\Red Alert 3\Maps -> data\maps\internal
            if (realPathsToVirtualPaths is not null)
            {
                foreach (var (real, @virtual) in realPathsToVirtualPaths)
                {
                    _realPathsToVirtualPaths[NormalizeFilePath(real)] = NormalizeFilePath(@virtual);
                }
            }

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
                        TryAddEntry(new FileSystemEntry(this, relativePath, (uint) new FileInfo(file).Length, () => File.OpenRead(file)));
                    }
                }

                // Then load .big files
                SkudefReader.Read(rootDirectory, path => AddBigArchive(path));
            }
            else
            {
                Logger.Warn("Failed to create filesystem for non existing root directory: " + rootDirectory);
            }
        }

        private string ResolveToVirtualPath(string path)
        {
            path = NormalizeFilePath(path);
            // check if file path should be converted to a virtual path
            // e.g.
            // [From] [Root Directory = %appdata%\red alert 3\]maps\somemap\somemap.map
            // [To] data\maps\internal\somemap\somemap.map
            foreach (var (real, @virtual) in _realPathsToVirtualPaths)
            {
                if (path.StartsWith(real))
                {
                    return @virtual + path[real.Length..];
                }
            }
            return path;
        }

        private void AddBigArchive(string path)
        {
            var archive = new BigArchive(path);

            _bigArchives.Add(archive);

            foreach (var entry in archive.Entries)
            {
                var filePath = NormalizeFilePath(entry.FullName);
                TryAddEntry(new FileSystemEntry(this, filePath, entry.Length, entry.Open));
            }
        }

        private void TryAddEntry(FileSystemEntry entry)
        {
            var virutalPath = ResolveToVirtualPath(entry.FilePath);
            if (!_fileTable.ContainsKey(virutalPath))
            {
                _fileTable.Add(virutalPath, entry);
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

        public void Update(FileSystemEntry entry)
        {
            if (entry.FileSystem != this)
            {
                throw new InvalidOperationException();
            }
            var virtualPath = ResolveToVirtualPath(entry.FilePath);
            _fileTable[virtualPath] = entry;
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
