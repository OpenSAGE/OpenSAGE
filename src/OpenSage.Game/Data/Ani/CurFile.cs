using System.IO;
using System.Text;

namespace OpenSage.Data.Ani
{
    public sealed class CurFile
    {
        public CursorImage Image { get; private set; }

        public static CurFile FromFileSystemEntry(FileSystemEntry entry)
        {
            using (var stream = entry.Open())
            using (var reader = new BinaryReader(stream, Encoding.ASCII, true))
            {
                var content = IconChunkContent.Parse(reader, stream.Length);

                return new CurFile
                {
                    Image = content.GetImage(0)
                };
            }
        }
    }
}
