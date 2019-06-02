using System.IO;
using OpenSage.FileFormats.RefPack;

namespace OpenSage.FileFormats.Big
{
    public class BigArchiveEntry
    {
        private readonly uint _offset;

        public BigArchive Archive { get; }

        public string FullName { get; }
        public string Name {
            get { return FullName.Split('\\')[FullName.Split('\\').Length - 1]; }
        }
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

                // Wrap RefPackStream in (another) BufferedStream, to improve performance.
                return new BufferedStream(refPackStream);
            }

            return bufferedBigStream;
        }
    }
}
