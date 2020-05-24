using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

        public BigArchiveMode Mode { get; }
        public long Size => _stream.Length;

        public IReadOnlyList<BigArchiveEntry> Entries => _entries;

        public BigArchiveVersion Version { get; private set; }

        public BigArchive(string filePath, BigArchiveMode mode = BigArchiveMode.Update)
        {
            FilePath = filePath;
            Mode = mode;

            _entries = new List<BigArchiveEntry>();
            _entriesDictionary = new Dictionary<string, BigArchiveEntry>();

            FileMode fileMode = mode == BigArchiveMode.Create ? FileMode.Create : FileMode.Open;
            FileAccess fileAccess = mode == BigArchiveMode.Read ? FileAccess.Read : FileAccess.ReadWrite;
            FileShare fileShare = mode == BigArchiveMode.Read ? FileShare.Read : FileShare.ReadWrite;

            _stream = AddDisposable(new FileStream(
                filePath,
                fileMode,
                fileAccess,
                fileShare));

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

        public BigArchiveEntry CreateEntry(string entryName)
        {
            var entry = new BigArchiveEntry(this, entryName);
            _entriesDictionary[entryName] = entry;
            _entries.Add(entry);
            return entry;
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

        internal void DeleteEntry(BigArchiveEntry entry)
        {
            _entries.Remove(entry);
        }

        private long CalculateContentSize()
        {
            long contentSize = 0;
            foreach (var entry in _entries)
            {
                contentSize += entry.Length;
            }

            return contentSize;
        }

        private int CalculateTableSize()
        {
            int tableSize = 0;
            foreach (var entry in _entries)
            {
                // Each entry has 4 bytes for the offset + 4 for size
                tableSize += 8;
                // And a null-terminated string
                tableSize += entry.FullName.Length + 1;
            }

            return tableSize;
        }

        private void WriteHeader(BinaryWriter bw, long archiveSize, int dataStart)
        {
            string fourCC = "";
            switch (Version)
            {
                case BigArchiveVersion.BigF:
                    fourCC = "BIGF";
                    break;
                case BigArchiveVersion.Big4:
                    fourCC = "BIG4";
                    break;
                default:
                    throw new InvalidDataException("Big archive version must be set");
            }

            bw.WriteFourCc(fourCC);
            bw.WriteBigEndianUInt32((uint) archiveSize);
            bw.WriteBigEndianUInt32((uint) _entries.Count);
            bw.WriteBigEndianUInt32((uint) dataStart);
        }

        private void UpdateOffsets()
        {
            foreach (var entry in _entries)
            {
                entry.Offset = entry.OutstandingOffset;
                entry.OutstandingOffset = 0;
            }
        }

        private void WriteFileTable(BinaryWriter bw, int dataStart)
        {
            long entryOffset = dataStart;
            foreach (var entry in _entries)
            {
                // Each entry has 4 bytes for the offset + 4 for size
                bw.WriteBigEndianUInt32((uint) entryOffset);
                bw.WriteBigEndianUInt32((uint) entry.Length);
                bw.WriteNullterminatedString(entry.FullName);

                entry.OutstandingOffset = (uint) entryOffset;
                entryOffset += entry.Length;
            }
        }

        private void WriteFileContent(BinaryWriter bw)
        {
            foreach (var entry in _entries)
            {
                using (var content = new MemoryStream())
                {
                    if (entry.OutstandingWriteStream != null)
                    {
                        entry.OutstandingWriteStream.WriteTo(content);
                    }
                    else
                    {
                        using (var stream = entry.Open())
                        {
                            // Each entry has 4 bytes for the offset + 4 for size
                            stream.CopyTo(content);
                        }
                    }
                    bw.BaseStream.Position = entry.OutstandingOffset;
                    var str = Encoding.ASCII.GetString(content.ToArray());
                    bw.Write(content.ToArray());
                }
            }
        }

        internal void WriteToDisk()
        {
            bool needsWrite = _entries.Any(x => x.OnDisk == false);

            if (needsWrite)
            {
                var outArchive = new MemoryStream();
                const int headerSize = 16;
                _entries.ForEach(x => x.OnDisk = true);
                int tableSize = CalculateTableSize();
                long contentSize = CalculateContentSize();
                long archiveSize = headerSize + tableSize + contentSize;
                int dataStart = headerSize + tableSize;
                outArchive.SetLength(archiveSize);

                using (var writer = new BinaryWriter(outArchive))
                {
                    WriteHeader(writer, archiveSize, dataStart);
                    WriteFileTable(writer, dataStart);
                    WriteFileContent(writer);
                    _stream.Position = 0;
                    _stream.SetLength(archiveSize);
                    outArchive.WriteTo(_stream);
                }

                UpdateOffsets();
            }
        }
    }

    public enum BigArchiveVersion
    {
        BigF,
        Big4
    }
}
