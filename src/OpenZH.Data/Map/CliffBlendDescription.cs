using System.IO;

namespace OpenZH.Data.Map
{
    public sealed class CliffBlendDescription
    {
        public uint Unknown1 { get; private set; }
        public byte[] Unknown2 { get; private set; }

        public static CliffBlendDescription Parse(BinaryReader reader)
        {
            var unknown1 = reader.ReadUInt32();
            if (unknown1 != 0x0000004C)
            {
                //throw new InvalidDataException();
            }

            var unknown2 = reader.ReadBytes(34);

            return new CliffBlendDescription
            {
                Unknown1 = unknown1,
                Unknown2 = unknown2
            };
        }

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(Unknown1);
            writer.Write(Unknown2);
        }
    }
}
