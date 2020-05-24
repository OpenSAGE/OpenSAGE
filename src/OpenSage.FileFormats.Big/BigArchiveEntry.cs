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

        internal bool OnDisk { get; set; }
        internal MemoryStream OutstandingWriteStream { get; set; }
        internal uint OutstandingOffset { get; set; }

        private bool _currentlyOpenForWrite;
        private bool _everOpenedForWrite;

        internal BigArchiveEntry(BigArchive archive, string name, uint offset, uint size)
        {
            Archive = archive;
            FullName = name;
            Offset = offset;
            Length = size;
            OnDisk = true;
            OutstandingOffset = 0;
            _currentlyOpenForWrite = false;
            _everOpenedForWrite = false;
        }

        internal BigArchiveEntry(BigArchive archive, string name)
        {
            Archive = archive;
            FullName = name;
            Length = 0;
            OnDisk = false;
            _currentlyOpenForWrite = false;
            _everOpenedForWrite = false;
        }

        public void Delete()
        {
            if (Archive == null)
                return;

            if (_currentlyOpenForWrite)
                throw new IOException("Entry is currently being written to");

            if (Archive.Mode != BigArchiveMode.Update)
                throw new NotSupportedException("Delete only works in update mode");

            Archive.DeleteEntry(this);
        }

        private Stream OpenInReadMode()
        {
            var bigStream = new BigArchiveEntryStream(this, Offset);

            // Wrapping BigStream in a BufferedStream significantly improves performance.
            var bufferedBigStream = new BufferedStream(bigStream);

            // Check for refpack compression header.
            // C&C3 started using refpack compression for .big archive entries.
            if (RefPackStream.IsProbablyRefPackCompressed(bufferedBigStream))
            {
                var refPackStream = new RefPackStream(bufferedBigStream);

                // Wrap RefPackStream in (another) BufferedStream, to improve performance.
                return new BufferedStream(refPackStream);
            }

            return bufferedBigStream;
        }

        private Stream OpenInWriteMode()
        {
            if (_everOpenedForWrite)
                throw new IOException("Can only write once and one entry at a time!");

            _everOpenedForWrite = true;

            var bigStream = new BigArchiveEntryStream(this, Offset);

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
            if (_everOpenedForWrite)
                throw new IOException("Can only write once and one entry at a time!");

            _everOpenedForWrite = true;
            _currentlyOpenForWrite = true;

            var bigStream = new BigArchiveEntryStream(this, Offset);

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
            if (OutstandingWriteStream != null)
            {
                OutstandingWriteStream.Dispose();
            }
        }
    }
}
