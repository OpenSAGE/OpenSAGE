using System.IO;

namespace OpenSage.FileFormats.W3d;

/// <param name="Pivot">Pivot affected by this channel</param>
/// <param name="DeltaType"></param>
/// <param name="VectorLength"></param>
/// <param name="ChannelType"></param>
/// <param name="NumTimeCodes"></param>
/// <param name="Data"></param>
public sealed record W3dMotionChannel(
    ushort Pivot,
    W3dMotionChannelDeltaType DeltaType,
    byte VectorLength,
    W3dAnimationChannelType ChannelType,
    ushort NumTimeCodes,
    IW3dMotionChannelData Data) : W3dChunk(W3dChunkType.W3D_CHUNK_COMPRESSED_ANIMATION_MOTION_CHANNEL)
{
    internal static W3dMotionChannel Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var zero = reader.ReadByte();
            if (zero != 0)
            {
                throw new InvalidDataException();
            }

            var deltaType = reader.ReadByteAsEnum<W3dMotionChannelDeltaType>();
            var vectorLength = reader.ReadByte();
            var channelType = reader.ReadByteAsEnum<W3dAnimationChannelType>();
            var numTimeCodes = reader.ReadUInt16();
            var pivot = reader.ReadUInt16();

            W3dAnimationChannel.ValidateChannelDataSize(channelType, vectorLength);

            IW3dMotionChannelData data = deltaType switch
            {
                W3dMotionChannelDeltaType.TimeCoded => W3dMotionChannelTimeCodedData.Parse(reader, numTimeCodes,
                    channelType),
                W3dMotionChannelDeltaType.Delta4 => W3dMotionChannelAdaptiveDeltaData.Parse(reader, numTimeCodes,
                    channelType, vectorLength, W3dAdaptiveDeltaBitCount.FourBits),
                W3dMotionChannelDeltaType.Delta8 => W3dMotionChannelAdaptiveDeltaData.Parse(reader, numTimeCodes,
                    channelType, vectorLength, W3dAdaptiveDeltaBitCount.EightBits),
                _ => throw new InvalidDataException(),
            };

            return new W3dMotionChannel(pivot, deltaType, vectorLength, channelType, numTimeCodes, data);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        writer.Write((byte)0);

        writer.Write((byte)DeltaType);
        writer.Write(VectorLength);
        writer.Write((byte)ChannelType);
        writer.Write(NumTimeCodes);
        writer.Write(Pivot);

        Data.WriteTo(writer, ChannelType);
    }
}
