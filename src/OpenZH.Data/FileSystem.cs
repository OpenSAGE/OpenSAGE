using System;
using System.Collections.Generic;
using System.IO;
using OpenZH.Data.Big;

namespace OpenZH.Data
{
    public sealed class FileSystem : IDisposable
    {
        private readonly Dictionary<string, FileSystemEntry> _fileTable;
        private readonly List<BigArchive> _bigArchives;

        public IReadOnlyCollection<FileSystemEntry> Files => _fileTable.Values;

        public FileSystem(string rootDirectory)
        {
            _fileTable = new Dictionary<string, FileSystemEntry>();
            _bigArchives = new List<BigArchive>();

            // TODO: Figure out if there's a specific order that .big files should be loaded in,
            // since some files are contained in more than one .big file so the later one
            // takes precedence over the earlier one.

            foreach (var file in Directory.GetFiles(rootDirectory, "*.*", SearchOption.AllDirectories))
            {
                var ext = Path.GetExtension(file).ToLower();
                if (ext == ".big")
                {
                    var bigStream = File.OpenRead(file);
                    var archive = new BigArchive(bigStream);

                    _bigArchives.Add(archive);

                    foreach (var entry in archive.Entries)
                    {
                        _fileTable[entry.FullName] = new FileSystemEntry(entry.FullName, entry.Length, entry.Open);
                    }
                }
                else
                {
                    var relativePath = file.Substring(rootDirectory.Length);
                    if (relativePath.StartsWith(Path.DirectorySeparatorChar.ToString()))
                        relativePath = relativePath.Substring(1);
                    _fileTable[relativePath] = new FileSystemEntry(relativePath, (uint) new FileInfo(file).Length, () => File.OpenRead(file));
                }
            }
        }

        public FileSystemEntry GetFile(string filePath)
        {
            if (_fileTable.TryGetValue(filePath, out var file))
                return file;
            return null;
        }

        public void Dispose()
        {
            foreach (var archive in _bigArchives)
                archive.Dispose();
            _bigArchives.Clear();
        }
    }
}
