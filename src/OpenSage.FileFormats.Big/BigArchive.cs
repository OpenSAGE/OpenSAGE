using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace OpenSage.FileFormats.Big
{
    [DebuggerDisplay("Archive: {FilePath}")]
    public class BigArchive : DisposableBase
    {
        private readonly object _lockObject = new object();

        private readonly FileStream _stream;

        private readonly List<BigArchiveEntry> _entries;
        private readonly Dictionary<string, BigArchiveEntry> _entriesDictionary;

        internal Stream Stream => _stream;

        public string FilePath { get; }

        public IReadOnlyList<BigArchiveEntry> Entries => _entries;

        public BigArchiveVersion Version { get; private set; }

        public BigArchive(string filePath)
        {
            FilePath = filePath;

            _entries = new List<BigArchiveEntry>();
            _entriesDictionary = new Dictionary<string, BigArchiveEntry>();

            _stream = AddDisposable(new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read));

            Read();
        }

        internal void AcquireLock()
        {
            Monitor.Enter(_lockObject);
        }

        internal void ReleaseLock()
        {
            Monitor.Exit(_lockObject);
        }

        private void Read()
        {
            using (var reader = new BinaryReader(_stream, Encoding.ASCII, true))
            {
                //Special case for empty archives/ placeholder archives
                if (reader.BaseStream.Length < 4)
                {
                    var a = reader.ReadByte();
                    var b = reader.ReadByte();

                    if (a == '?' && b == '?')
                    {
                        return;
                    }
                    else
                    {
                        throw new InvalidDataException($"Big archive is too small");
                    }
                }

                var fourCc = reader.ReadFourCc();
                switch (fourCc)
                {
                    case "BIGF":
                        Version = BigArchiveVersion.BigF;
                        break;

                    case "BIG4":
                        Version = BigArchiveVersion.Big4;
                        break;

                    default:
                        throw new InvalidDataException($"Not a supported BIG format: {fourCc}");
                }

                reader.ReadBigEndianUInt32(); // Archive Size
                var numEntries = reader.ReadBigEndianUInt32();
                reader.ReadBigEndianUInt32(); // First File Offset

                for (var i = 0; i < numEntries; i++)
                {
                    var entryOffset = reader.ReadBigEndianUInt32();
                    var entrySize = reader.ReadBigEndianUInt32();
                    var entryName = reader.ReadNullTerminatedString();

                    var entry = new BigArchiveEntry(this, entryName, entryOffset, entrySize);

                    _entries.Add(entry);

                    // Overwrite any previous entries with the same name.
                    // Yes, at least one .big file has entries with duplicate names.
                    _entriesDictionary[entryName] = entry;
                }
            }
        }

        public BigArchiveEntry GetEntry(string entryName)
        {
            if (entryName == null)
            {
                throw new ArgumentNullException(nameof(entryName));
            }

            _entriesDictionary.TryGetValue(entryName, out var result);
            return result;
        }
    }

    public enum BigArchiveVersion
    {
        BigF,
        Big4
    }
}
