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
        internal MemoryStream WriteBuffer { get; set; }

        private Stream _stream;

        internal BigArchiveEntry(BigArchive archive, string name, uint offset, uint size)
        {
            Archive = archive;
            FullName = name;
            Offset = offset;
            Length = size;
            OnDisk = true;
        }

        internal BigArchiveEntry(BigArchive archive, string name)
        {
            Archive = archive;
            FullName = name;
            Length = 0;
            OnDisk = false;
        }

        public void Delete()
        {
            Archive.DeleteEntry(this);
        }

        public Stream Open()
        {
            if(WriteBuffer!=null)
            {
                return WriteBuffer;
            }
                
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
    }
}
