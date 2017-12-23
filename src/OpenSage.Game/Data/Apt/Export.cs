using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Apt
{
    public sealed class Export
    {
        public string Name { get; private set; }
        public uint Character { get; private set; }

        public static Export Parse(BinaryReader reader)
        {
            var export = new Export();
            export.Name = reader.ReadStringAtOffset();
            export.Character = reader.ReadUInt32();

            return export;
        }
    }
}
