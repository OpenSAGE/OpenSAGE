using System;
using System.IO;

namespace OpenZH.Data
{
    public sealed class FileSystemEntry
    {
        private readonly Func<Stream> _open;

        public string FilePath { get; }
        public uint Length { get; }

        public FileSystemEntry(string filePath, uint length, Func<Stream> open)
        {
            FilePath = filePath;
            Length = length;
            _open = open;
        }

        public Stream Open() => _open();
    }
}
