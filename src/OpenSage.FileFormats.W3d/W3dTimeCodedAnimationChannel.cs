using System.Diagnostics;
using System.IO;

namespace OpenSage.FileFormats.W3d;

/// <param name="NumTimeCodes"></param>
/// <param name="Pivot">Pivot affected by this channel</param>
/// <param name="VectorLength">Length of each vector in this channel</param>
/// <param name="ChannelType"></param>
/// <param name="Data"></param>
public sealed record W3dTimeCodedAnimationChannel(
    uint NumTimeCodes,
    ushort Pivot,
    byte VectorLength,
    W3dAnimationChannelType ChannelType,
    W3dTimeCodedDatum[] Data) : W3dChunk(W3dChunkType.W3D_CHUNK_COMPRESSED_ANIMATION_CHANNEL)
{
    internal static W3dTimeCodedAnimationChannel Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var numTimeCodes = reader.ReadUInt32();
            var pivot = reader.ReadUInt16();
            var vectorLength = reader.ReadByte();
            var channelType = reader.ReadByteAsEnum<W3dAnimationChannelType>();

            W3dAnimationChannel.ValidateChannelDataSize(channelType, vectorLength);

            var data = new W3dTimeCodedDatum[numTimeCodes];
            for (var i = 0; i < numTimeCodes; i++)
            {
                data[i] = W3dTimeCodedDatum.Parse(reader, channelType);
            }

            return new W3dTimeCodedAnimationChannel(numTimeCodes, pivot, vectorLength, channelType, data);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        writer.Write(NumTimeCodes);
        writer.Write(Pivot);
        writer.Write(VectorLength);
        writer.Write((byte)ChannelType);

        for (var i = 0; i < Data.Length; i++)
        {
            Data[i].WriteTo(writer, ChannelType);
        }
    }
}

[DebuggerDisplay("TimeCode = {TimeCode}, Value = {Value}")]
public sealed record W3dTimeCodedDatum(
    uint TimeCode,
    bool NonInterpolatedMovement,
    W3dAnimationChannelDatum Value)
{
    internal static W3dTimeCodedDatum Parse(BinaryReader reader, W3dAnimationChannelType channelType)
    {
        var timeCode = reader.ReadUInt32();
        var nonInterpolatedMovement = false;

        // MSB is used to indicate a binary (non interpolated) movement
        if ((timeCode >> 31) == 1)
        {
            nonInterpolatedMovement = true;
            // TODO: non-interpolated movement.

            timeCode &= ~(1 << 31);
        }

        var value = W3dAnimationChannelDatum.Parse(reader, channelType);

        return new W3dTimeCodedDatum(timeCode, nonInterpolatedMovement, value);
    }

    internal void WriteTo(BinaryWriter writer, W3dAnimationChannelType channelType)
    {
        var timeCode = TimeCode;
        if (NonInterpolatedMovement)
        {
            timeCode |= (1u << 31);
        }
        writer.Write(timeCode);

        Value.WriteTo(writer, channelType);
    }
}
