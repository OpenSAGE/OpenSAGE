using System.IO;
using OpenSage.Data.Utilities.Extensions;
using OpenSage.FileFormats;

namespace OpenSage.Data.Apt
{
    public sealed class Export
    {
        public string Name { get; private set; }
        public uint Character { get; private set; }

        public static Export Parse(BinaryReader reader)
        {
            return new Export
            {
                Name = reader.ReadStringAtOffset(),
                Character = reader.ReadUInt32()
            };
        }

        public static Export Create(string name, int character)
        {
            return new Export
            {
                Name = name,
                Character = (uint) character
            };
        }
    }
}
