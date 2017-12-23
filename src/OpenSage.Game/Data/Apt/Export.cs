using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Apt
{
    public class Export
    {
        public string Name { get; private set; }
        public uint Character { get; private set; }

        public static Export Parse(BinaryReader reader)
        {
            var ex = new Export();
            ex.Name = reader.ReadStringAtOffset();
            ex.Character = reader.ReadUInt32();

            return ex;
        }
    }
}
