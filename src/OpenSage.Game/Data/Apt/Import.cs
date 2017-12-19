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

        public static Import Parse(BinaryReader br)
        {
            var im = new Import();
            im.Movie = br.ReadStringAtOffset();
            im.Name = br.ReadStringAtOffset();
            im.Character = br.ReadUInt32();
            im.Pointer = br.ReadUInt32();

            return im;
        }
    }
}
