using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using OpenSage.FileFormats.RefPack;

namespace OpenSage.FileFormats.Big
{
    public class BigArchiveEntry
    {
        internal uint Offset;

        public BigArchive Archive { get; }

        public string FullName { get; }
        public string Name => FullName.Split('\\').Last();
        public uint Length { get; internal set; }

        // Internal members
        internal bool OnDisk { get; set; }
        internal MemoryStream OutstandingWriteStream { get; set; }
        internal uint OutstandingOffset { get; set; }
        internal bool CurrentlyOpenForWrite { get; set; }
        internal BigArchiveEntry(BigArchive archive, string name, uint offset, uint size)
        {
            Archive = archive;
            FullName = name;
            Offset = offset;
            Length = size;
            OnDisk = true;
            OutstandingOffset = 0;
            CurrentlyOpenForWrite = false;
        }

        internal BigArchiveEntry(BigArchive archive, string name)
        {
            Archive = archive;
            FullName = name;
            Length = 0;
            OnDisk = false;
            CurrentlyOpenForWrite = false;
        }

        public void Delete()
        {
            if (Archive == null)
                return;

            if (CurrentlyOpenForWrite)
                throw new IOException("Entry is currently being written to");

            if (Archive.Mode != BigArchiveMode.Update)
                throw new NotSupportedException("Delete only works in update mode");

            Archive.DeleteEntry(this);
        }

        private Memory<byte> GetMemorySpan()
        {
            return Archive.Stream.Memory.Slice((int) Offset, (int) Length);
        }

        private Stream OpenInReadMode()
        {
            var mem = GetMemorySpan();
            var bigStream = new BigArchiveEntryStream(this, mem);

            // Check for refpack compression header.
            // C&C3 started using refpack compression for .big archive entries.
            if (RefPackStream.IsProbablyRefPackCompressed(bigStream))
            {
                return new RefPackStream(bigStream);
            }

            return bigStream;
        }

        private Stream OpenInWriteMode()
        {
            if (CurrentlyOpenForWrite)
                throw new IOException("Can only write once and one entry at a time!");

            CurrentlyOpenForWrite = true;

            var mem = GetMemorySpan();
            var bigStream = new BigArchiveEntryStream(this, mem);

            // Check for refpack compression header.
            // C&C3 started using refpack compression for .big archive entries.
            if (RefPackStream.IsProbablyRefPackCompressed(bigStream))
            {
                return new RefPackStream(bigStream);
            }

            return bigStream;
        }

        private Stream OpenInUpdateMode()
        {
            if (CurrentlyOpenForWrite)
                throw new IOException("Can only open a single write stream!");

            CurrentlyOpenForWrite = true;

            var mem = GetMemorySpan();
            var bigStream = new BigArchiveEntryStream(this, mem);

            // Check for refpack compression header.
            // C&C3 started using refpack compression for .big archive entries.
            if (RefPackStream.IsProbablyRefPackCompressed(bigStream))
            {
                return new RefPackStream(bigStream);
            }

            return bigStream;
        }

        public Stream Open()
        {
            switch (Archive.Mode)
            {
                case BigArchiveMode.Read:
                    return OpenInReadMode();
                case BigArchiveMode.Create:
                    return OpenInWriteMode();
                case BigArchiveMode.Update:
                default:
                    Debug.Assert(Archive.Mode == BigArchiveMode.Update);
                    return OpenInUpdateMode();
            }
        }

        internal void CloseStreams()
        {
            // if the user left the stream open, close the underlying stream for them
            OutstandingWriteStream?.Dispose();
            OutstandingWriteStream = null;
        }
    }
}
