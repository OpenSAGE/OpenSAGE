using System.IO;

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
            return new BigStream(this, _offset);
        }
    }
}
