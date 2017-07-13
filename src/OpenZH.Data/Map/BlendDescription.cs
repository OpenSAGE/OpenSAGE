using System;
using System.IO;
using OpenZH.Data.Utilities.Extensions;

namespace OpenZH.Data.Map
{
    public sealed class BlendDescription
    {
        public uint SecondaryTextureTile { get; private set; }
        public BlendDirection BlendDirection { get; private set; }
        public BlendFlags Flags { get; private set; }
        public bool Unknown1 { get; private set; }
        public uint Unknown2 { get; private set; }
        public uint Unknown3 { get; private set; }

        public static BlendDescription Parse(BinaryReader reader)
        {
            var secondaryTextureTile = reader.ReadUInt32();

            var blendDirection = ToBlendDirection(reader.ReadBytes(4));

            var flags = reader.ReadByteAsEnum<BlendFlags>();

            var unknown1 = reader.ReadBoolean();

            var unknown2 = reader.ReadUInt32();
            if (unknown2 != 0xFFFFFFFF)
            {
                throw new InvalidDataException();
            }

            var unknown3 = reader.ReadUInt32();
            if (unknown3 != 0x7ADA0000)
            {
                throw new InvalidDataException();
            }

            return new BlendDescription
            {
                SecondaryTextureTile = secondaryTextureTile,
                BlendDirection = blendDirection,
                Flags = flags,
                Unknown1 = unknown1,
                Unknown2 = unknown2,
                Unknown3 = unknown3
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

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(SecondaryTextureTile);

            writer.Write(ToBytes(BlendDirection));

            writer.Write((byte) Flags);

            writer.Write(Unknown1);

            writer.Write(Unknown2);
            writer.Write(Unknown3);
        }
    }

    [Flags]
    public enum BlendFlags : byte
    {
        None = 0,
        Reversed = 1,
        Unknown = 2,
        Reversed_Unknown = Reversed | Unknown
    }
}
