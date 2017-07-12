using System.IO;

namespace OpenZH.Data.Map
{
    public sealed class BlendDescription
    {
        public uint SecondaryTextureTile { get; private set; }
        public BlendDirection BlendDirection { get; private set; }
        public uint Unknown1 { get; private set; }
        public uint Unknown2 { get; private set; }

        public static BlendDescription Parse(BinaryReader reader)
        {
            var secondaryTextureTile = reader.ReadUInt32();

            var blendDirection = reader.ReadBytes(6);

            var unknown1 = reader.ReadUInt32();
            if (unknown1 != 0xFFFFFFFF)
            {
                throw new InvalidDataException();
            }

            var unknown2 = reader.ReadUInt32();
            if (unknown2 != 0x7ADA0000)
            {
                throw new InvalidDataException();
            }

            return new BlendDescription
            {
                SecondaryTextureTile = secondaryTextureTile,
                BlendDirection = ToBlendDirection(blendDirection),
                Unknown1 = unknown1,
                Unknown2 = unknown2
            };
        }

        private static BlendDirection ToBlendDirection(byte[] bytes)
        {
            var result = 0;

            for (int i = 0; i < bytes.Length; i++)
            {
                if (bytes[i] == 1)
                {
                    result |= 1 << i;
                }
            }

            return (BlendDirection) result;
        }

        private static byte[] ToBytes(BlendDirection value)
        {
            var result = new byte[6];

            for (var i = 0; i < result.Length; i++)
            {
                result[i] = (byte) ((((int) value) >> i) & 0x1);
            }

            return result;
        }

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(SecondaryTextureTile);

            writer.Write(ToBytes(BlendDirection));

            writer.Write(Unknown1);
            writer.Write(Unknown2);
        }
    }
}
