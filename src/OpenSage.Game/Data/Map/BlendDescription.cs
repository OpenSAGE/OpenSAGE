using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Map
{
    public sealed class BlendDescription
    {
        public uint SecondaryTextureTile { get; private set; }
        public BlendDirection BlendDirection { get; private set; }
        public BlendFlags Flags { get; private set; }

        /// <summary>
        /// True if blending from a corner, and blending should also occur from the
        /// adjacent horizontal and vertical sides.
        /// </summary>
        public bool TwoSided { get; private set; }

        public uint MagicValue1 { get; private set; }
        public uint MagicValue2 { get; private set; }

        internal static BlendDescription Parse(BinaryReader reader)
        {
            var secondaryTextureTile = reader.ReadUInt32();

            var blendDirection = ToBlendDirection(reader.ReadBytes(4));

            var flags = reader.ReadByteAsEnum<BlendFlags>();

            var twoSided = reader.ReadBooleanChecked();

            var magicValue1 = reader.ReadUInt32();
            if (magicValue1 != 0xFFFFFFFF)
            {
                throw new InvalidDataException();
            }

            var magicValue2 = reader.ReadUInt32();
            if (magicValue2 != 0x7ADA0000)
            {
                throw new InvalidDataException();
            }

            return new BlendDescription
            {
                SecondaryTextureTile = secondaryTextureTile,
                BlendDirection = blendDirection,
                Flags = flags,
                TwoSided = twoSided,
                MagicValue1 = magicValue1,
                MagicValue2 = magicValue2
            };
        }

        private static BlendDirection ToBlendDirection(byte[] bytes)
        {
            var result = 0;

            for (int i = 0; i < bytes.Length; i++)
            {
                result |= bytes[i] << i;
            }

            return (BlendDirection) result;
        }

        private static byte[] ToBytes(BlendDirection value)
        {
            var result = new byte[4];

            var byteValue = (byte) value;

            for (var i = 0; i < result.Length; i++)
            {
                result[i] = (byte) (value.HasFlag((BlendDirection) (1 << i)) ? 1 : 0);
            }

            return result;
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.Write(SecondaryTextureTile);

            writer.Write(ToBytes(BlendDirection));

            writer.Write((byte) Flags);

            writer.Write(TwoSided);

            writer.Write(MagicValue1);
            writer.Write(MagicValue2);
        }
    }
}
