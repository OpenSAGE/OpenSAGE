﻿using System.IO;
using System.Linq;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Map;

public sealed class BlendDescription
{
    private const uint MagicValue1_1 = 0xFFFFFFFF;
    private const uint MagicValue1_2 = 24;

    private const uint MagicValue2 = 0x7ADA0000;

    public uint SecondaryTextureTile { get; private set; }
    public BlendDirection BlendDirection { get; private set; }

    // TODO: Here until I figure out what changed in BlendDirection in v15...
    public byte[] RawBlendDirection { get; private set; }

    public BlendFlags Flags { get; private set; }

    /// <summary>
    /// True if blending from a corner, and blending should also occur from the
    /// adjacent horizontal and vertical sides.
    /// </summary>
    public bool TwoSided { get; private set; }

    public uint MagicValue1 { get; private set; }

    internal static BlendDescription Parse(BinaryReader reader, ushort version)
    {
        var secondaryTextureTile = reader.ReadUInt32();

        // TODO: These bytes are all 221 for at least one RA3 map.
        var rawBlendDirection = reader.ReadBytes(4);
        var blendDirection = ToBlendDirection(rawBlendDirection);

        // TODO: Figure out these flags for BFME I maps.
        // TODO: This is 221 for at least one RA3 map.
        var flags = (BlendFlags)reader.ReadByte();
        //var flags = reader.ReadByteAsEnum<BlendFlags>();

        // TODO: This is 221 for at least one RA3 map.
        var twoSided = reader.ReadBoolean(); // reader.ReadBooleanChecked();

        var magicValue1 = reader.ReadUInt32();
        if (magicValue1 != MagicValue1_1 && magicValue1 != MagicValue1_2)
        {
            // TODO: What is this?
            //throw new InvalidDataException();
        }

        var magicValue2 = reader.ReadUInt32();
        if (magicValue2 != MagicValue2)
        {
            throw new InvalidDataException();
        }

        return new BlendDescription
        {
            SecondaryTextureTile = secondaryTextureTile,
            RawBlendDirection = rawBlendDirection,
            BlendDirection = blendDirection,
            Flags = flags,
            TwoSided = twoSided,
            MagicValue1 = magicValue1
        };
    }

    private static BlendDirection ToBlendDirection(byte[] bytes)
    {
        var result = 0;

        for (int i = 0; i < bytes.Length; i++)
        {
            if (bytes[i] != 0 && bytes[i] != 1)
            {
                // TODO
                //throw new InvalidDataException();
            }
            if (bytes[i] != 0)
            {
                result |= bytes[i] << i;
            }
        }

        return (BlendDirection)result;
    }

    private static byte[] ToBytes(BlendDirection value)
    {
        var result = new byte[4];

        var byteValue = (byte)value;

        for (var i = 0; i < result.Length; i++)
        {
            result[i] = (byte)(value.HasFlag((BlendDirection)(1 << i)) ? 1 : 0);
        }

        return result;
    }

    internal void WriteTo(BinaryWriter writer)
    {
        writer.Write(SecondaryTextureTile);

        writer.Write(RawBlendDirection);

        writer.Write((byte)Flags);

        writer.Write(TwoSided);

        writer.Write(MagicValue1);
        writer.Write(MagicValue2);
    }
}
