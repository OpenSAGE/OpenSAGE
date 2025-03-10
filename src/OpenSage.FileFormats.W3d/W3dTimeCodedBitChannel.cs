using System.Diagnostics;
using System.IO;

namespace OpenSage.FileFormats.W3d;

/// <param name="NumTimeCodes"></param>
/// <param name="Pivot">Pivot affected by this channel</param>
/// <param name="ChannelType"></param>
/// <param name="DefaultValue"></param>
/// <param name="Data"></param>
public sealed record W3dTimeCodedBitChannel(
    uint NumTimeCodes,
    ushort Pivot,
    W3dBitChannelType ChannelType,
    bool DefaultValue,
    W3dTimeCodedBitDatum[] Data) : W3dChunk(W3dChunkType.W3D_CHUNK_COMPRESSED_BIT_CHANNEL)
{
    internal static W3dTimeCodedBitChannel Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var numTimeCodes = reader.ReadUInt32();
            var pivot = reader.ReadUInt16();
            var channelType = reader.ReadByteAsEnum<W3dBitChannelType>();
            var defaultValue = reader.ReadBooleanChecked();

            var data = new W3dTimeCodedBitDatum[numTimeCodes];

            for (var i = 0; i < numTimeCodes; i++)
            {
                data[i] = W3dTimeCodedBitDatum.Parse(reader);
            }

            return new W3dTimeCodedBitChannel(numTimeCodes, pivot, channelType, defaultValue, data);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        writer.Write(NumTimeCodes);
        writer.Write(Pivot);
        writer.Write((byte)ChannelType);
        writer.Write(DefaultValue);

        for (var i = 0; i < Data.Length; i++)
        {
            Data[i].WriteTo(writer);
        }
    }
}

[DebuggerDisplay("TimeCode = {TimeCode}, Value = {Value}")]
public sealed record W3dTimeCodedBitDatum(uint TimeCode, bool Value)
{
    internal static W3dTimeCodedBitDatum Parse(BinaryReader reader)
    {
        var timecode = reader.ReadUInt32();

        // TODO: Verify this guess.
        var value = false;
        if ((timecode >> 31) == 1)
        {
            value = true;

            timecode &= ~(1 << 31);
        }

        return new W3dTimeCodedBitDatum(timecode, value);
    }

    internal void WriteTo(BinaryWriter writer)
    {
        var valueToWrite = TimeCode;

        if (Value)
        {
            valueToWrite |= (1u << 31);
        }

        writer.Write(valueToWrite);
    }
}
