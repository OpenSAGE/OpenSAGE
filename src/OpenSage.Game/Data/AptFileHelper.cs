using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using OpenSage.FileFormats.Apt.Characters;
using OpenSage.FileFormats;
using OpenSage.FileFormats.Apt;
using OpenSage.IO;

namespace OpenSage.Data
{
    public sealed class AptFileHelper
    {
        public FileSystem FileSystem { get; }
        public ConstantStorage Constants { get; }
        public string MovieName { get; }

        public Movie Movie { get; private set; }
        public ImageMap ImageMap { get; private set; }
        public Dictionary<uint, Geometry> GeometryMap { get; private set; }
        public Dictionary<string, FileSystemEntry> ImportMap { get; private set; }

        internal bool IsEmpty = true;

        class _fec : IEqualityComparer<FileSystemEntry>
        {
            public bool Equals(FileSystemEntry b1, FileSystemEntry b2)
            {
                return b1.ToString().Equals(b2.ToString());
            }

            public int GetHashCode(FileSystemEntry bx)
            {
                return bx.ToString().GetHashCode();
            }
        }

        private static _fec __fec = new _fec();
        private static Dictionary<FileSystemEntry, AptFile> _cache = new Dictionary<FileSystemEntry, AptFile>(__fec);
        private static List<FileSystemEntry> _recent = new List<FileSystemEntry>();
        private static readonly int _max_count = 12;
        public static AptFile FromFileSystemEntry(FileSystemEntry entry)
        {
            if (!_cache.ContainsKey(entry)) {
                _cache[entry] = FromFileSystemEntryRaw(entry);
                _recent.Add(entry);
                if (_recent.Count > _max_count)
                {
                    _cache.Remove(_recent[0]);
                    _recent.Remove(_recent[0]);
                }   
            }
            else
            {
                foreach (var v in _recent)
                    if (__fec.Equals(v, entry))
                    {
                        _recent.Remove(v);
                        break;
                    }
                _recent.Add(entry);
            }
            return _cache[entry];
        }
        public static AptFile FromFileSystemEntryRaw(FileSystemEntry entry)
        {
            var aptPath = entry.FilePath;

            Func<string, FileMode, Stream> getter = (path, mode) =>
            {
                if (mode != FileMode.Open)
                    throw new InvalidOperationException();

                var targetEntry = entry;
                if (path != aptPath)
                    targetEntry = entry.FileSystem.GetFile(path);
                if (targetEntry == null)
                    return null;
                return targetEntry.Open();
            };
            var getterWrapped = new StandardStreamGetter(aptPath, getter);
            return AptFile.Parse(getterWrapped);
        }
    }
}
