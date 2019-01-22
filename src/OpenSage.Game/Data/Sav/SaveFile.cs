using System.IO;
using System.Text;
using OpenSage.Data.Utilities.Extensions;
using OpenSage.FileFormats;

namespace OpenSage.Data.Sav
{
    public sealed class SaveFile
    {
        public static SaveFile FromFileSystemEntry(FileSystemEntry entry)
        {
            using (var stream = entry.Open())
            using (var reader = new BinaryReader(stream, Encoding.Unicode, true))
            {
                var chunkName = reader.ReadBytePrefixedAsciiString();

                var unknown = reader.ReadBytes(9);

                var mapName = reader.ReadBytePrefixedAsciiString();

                return new SaveFile();
            }
        }
    }
}
