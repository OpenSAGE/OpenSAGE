using System.IO;

namespace OpenSage.FileFormats.W3d;

/// <param name="FirstFrame"></param>
/// <param name="LastFrame"></param>
/// <param name="ChannelType"></param>
/// <param name="Pivot">Pivot affected by this channel</param>
/// <param name="DefaultValue"></param>
/// <param name="Data"></param>
public sealed record W3dBitChannel(
    ushort FirstFrame,
    ushort LastFrame,
    W3dBitChannelType ChannelType,
    ushort Pivot,
    bool DefaultValue,
    bool[] Data) : W3dAnimationChannelBase(W3dChunkType.W3D_CHUNK_BIT_CHANNEL)
{
    internal static W3dBitChannel Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var firstFrame = reader.ReadUInt16();
            var lastFrame = reader.ReadUInt16();
            var channelType = reader.ReadUInt16AsEnum<W3dBitChannelType>();
            var pivot = reader.ReadUInt16();
            var defaultValue = reader.ReadBooleanChecked();

            var numElements = lastFrame - firstFrame + 1;
            var data = reader.ReadSingleBitBooleanArray((uint)numElements);

            return new W3dBitChannel(firstFrame, lastFrame, channelType, pivot, defaultValue, data);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        writer.Write(FirstFrame);
        writer.Write(LastFrame);
        writer.Write((ushort)ChannelType);
        writer.Write(Pivot);
        writer.Write(DefaultValue);
        writer.WriteSingleBitBooleanArray(Data);
    }
}
