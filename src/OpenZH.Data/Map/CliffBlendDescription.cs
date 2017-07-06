using System.IO;

namespace OpenZH.Data.Map
{
    public sealed class CliffBlendDescription
    {
        public uint Unknown1 { get; private set; }
        public byte[] Unknown2 { get; private set; }

        public static CliffBlendDescription Parse(BinaryReader reader)
        {
            var unknown = reader.ReadUInt32();
            if (unknown != 0x0000004C)
            {
                //throw new InvalidDataException();
            }

            var unknown2 = reader.ReadBytes(34);

            return new CliffBlendDescription
            {
                Unknown1 = unknown,
                Unknown2 = unknown2
            };
        }
    }
}
