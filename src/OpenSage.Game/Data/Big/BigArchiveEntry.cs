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
            // TODO: Use System.IO.MemoryMappedFiles
            using (var bigStream = new BigStream(this, _offset))
            {
                var result = new MemoryStream((int) Length);

                bigStream.CopyTo(result);

                result.Position = 0;

                // Check for refpack compression header.
                // C&C3 started using refpack compression for .big archive entries.
                if (RefPackStream.IsProbablyRefPackCompressed(result.GetBuffer()))
                {
                    return new RefPackStream(result);
                }

                return result;
            }
        }
    }
}
