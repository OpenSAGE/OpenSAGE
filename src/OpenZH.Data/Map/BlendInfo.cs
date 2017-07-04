using System.IO;

namespace OpenZH.Data.Map
{
    public sealed class BlendInfo
    {
        public uint SecondaryTextureTile { get; private set; }
        public byte[] BlendDirectionBytes { get; private set; }
        public BlendDirection BlendDirection { get; private set; }

        public static BlendInfo Parse(BinaryReader reader)
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

            return new BlendInfo
            {
                SecondaryTextureTile = secondaryTextureTile,
                BlendDirectionBytes = blendDirection,
                BlendDirection = ToBlendDirection(blendDirection)
            };
        }

        private static BlendDirection ToBlendDirection(byte[] bytes)
        {
            int result = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                if (bytes[i] == 1)
                {
                    result |= 1 << i;
                }
            }

            return (BlendDirection) result;
        }
    }
}
