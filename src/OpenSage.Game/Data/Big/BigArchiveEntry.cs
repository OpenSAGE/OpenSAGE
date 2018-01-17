using System.IO;
using OpenSage.Data.RefPack;

namespace OpenSage.Data.Big
{
    public class BigArchiveEntry
    {
        private readonly uint _offset;

        public BigArchive Archive { get; }

        public string FullName { get; }
        public uint Length { get; }

        public BigArchiveEntry(BigArchive archive, string name, uint offset, uint size)
        {
            Archive = archive;
            FullName = name;
            _offset = offset;
            Length = size;
        }

        public Stream Open()
        {
            var bigStream = new BigArchiveEntryStream(this, _offset);

            // Wrapping BigStream in a BufferedStream significantly improves performance.
            var bufferedBigStream = new BufferedStream(bigStream);

            // Check for refpack compression header.
            // C&C3 started using refpack compression for .big archive entries.
            if (RefPackStream.IsProbablyRefPackCompressed(bufferedBigStream))
            {
                var refPackStream = new RefPackStream(bufferedBigStream);

                // TODO: Could wrap RefPackStream in (another) BufferedStream, to improve performance.
                // But we'd need to implement a proper Seek method on RefPackStream.
                return refPackStream;
            }

            return bufferedBigStream;
        }
    }
}
