using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dAdaptiveDeltaData(
    W3dAdaptiveDeltaBitCount BitCount,
    int VectorLength,
    W3dAnimationChannelDatum InitialDatum,
    W3dAdaptiveDeltaBlock[] DeltaBlocks)
{
    internal static W3dAdaptiveDeltaData Parse(
        BinaryReader reader,
        uint numFrames,
        W3dAnimationChannelType type,
        int vectorLength,
        W3dAdaptiveDeltaBitCount bitCount)
    {
        var count = (numFrames + 15) >> 4;

        // First read all initial values
        var datum = W3dAnimationChannelDatum.Parse(reader, type);

        var numBits = (int)bitCount;

        // Then read the interleaved delta blocks
        var deltaBlocks = new W3dAdaptiveDeltaBlock[count * vectorLength];
        for (var i = 0; i < count; i++)
        {
            for (var j = 0; j < vectorLength; j++)
            {
                deltaBlocks[(i * vectorLength) + j] = W3dAdaptiveDeltaBlock.Parse(
                    reader,
                    j,
                    numBits);
            }
        }

        var result = new W3dAdaptiveDeltaData(bitCount, vectorLength, datum, deltaBlocks);

        return result;
    }

    internal void WriteTo(BinaryWriter writer, W3dAnimationChannelType channelType)
    {
        InitialDatum.WriteTo(writer, channelType);

        for (var i = 0; i < DeltaBlocks.Length; i++)
        {
            DeltaBlocks[i].WriteTo(writer);
        }
    }
}
