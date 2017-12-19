using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Apt
{
    public class Export
    {
        public string Name { get; private set; }
        public uint Character { get; private set; }

        public static Export Parse(BinaryReader br)
        {
            var ex = new Export();
            ex.Name = br.ReadStringAtOffset();
            ex.Character = br.ReadUInt32();

            return ex;
        }
    }
}
