using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Apt
{
    public class Import
    {
        public string Movie { get; private set; }
        public string Name { get; private set; }
        public uint Character { get; private set; }
        public uint Pointer { get; private set; }

        public static Import Parse(BinaryReader reader)
        {
            var im = new Import();
            im.Movie = reader.ReadStringAtOffset();
            im.Name = reader.ReadStringAtOffset();
            im.Character = reader.ReadUInt32();
            im.Pointer = reader.ReadUInt32();

            return im;
        }
    }
}
