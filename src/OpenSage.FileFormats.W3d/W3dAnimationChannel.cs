using System.IO;

namespace OpenSage.FileFormats.W3d;

/// <param name="FirstFrame"></param>
/// <param name="LastFrame"></param>
/// <param name="VectorLength">Length of each vector in this channel</param>
/// <param name="ChannelType"></param>
/// <param name="Pivot">Pivot affected by this channel</param>
/// <param name="Unknown">Maybe padding?</param>
/// <param name="Data"></param>
/// <param name="NumPadBytes"></param>
public sealed record W3dAnimationChannel(
    ushort FirstFrame,
    ushort LastFrame,
    ushort VectorLength,
    W3dAnimationChannelType ChannelType,
    ushort Pivot,
    ushort Unknown,
    W3dAnimationChannelDatum[] Data,
    uint NumPadBytes) : W3dAnimationChannelBase(W3dChunkType.W3D_CHUNK_ANIMATION_CHANNEL)
{
    internal static W3dAnimationChannel Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var startPosition = reader.BaseStream.Position;

            var firstFrame = reader.ReadUInt16();
            var lastFrame = reader.ReadUInt16();
            var vectorLength = reader.ReadUInt16();
            var channelType = reader.ReadUInt16AsEnum<W3dAnimationChannelType>();
            var pivot = reader.ReadUInt16();
            var unknown = reader.ReadUInt16();

            ValidateChannelDataSize(channelType, vectorLength);

            var numElements = lastFrame - firstFrame + 1;
            var data = new W3dAnimationChannelDatum[numElements];

            for (var i = 0; i < numElements; i++)
            {
                data[i] = W3dAnimationChannelDatum.Parse(reader, channelType);
            }

            var numPadBytes = (uint)(context.CurrentEndPosition - reader.BaseStream.Position);
            reader.BaseStream.Seek((int)numPadBytes, SeekOrigin.Current);

            return new W3dAnimationChannel(firstFrame, lastFrame, vectorLength, channelType, pivot, unknown, data, numPadBytes);
        });
    }

    internal static void ValidateChannelDataSize(W3dAnimationChannelType channelType, int vectorLength)
    {
        switch (channelType)
        {
            case W3dAnimationChannelType.Quaternion:
                if (vectorLength != 4)
                {
                    throw new InvalidDataException();
                }
                break;

            case W3dAnimationChannelType.TranslationX:
            case W3dAnimationChannelType.TranslationY:
            case W3dAnimationChannelType.TranslationZ:
                if (vectorLength != 1)
                {
                    throw new InvalidDataException();
                }
                break;

            case W3dAnimationChannelType.UnknownBfme:
                if (vectorLength != 1)
                {
                    throw new InvalidDataException();
                }
                break;

            default:
                throw new InvalidDataException();
        }
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        writer.Write(FirstFrame);
        writer.Write(LastFrame);
        writer.Write(VectorLength);
        writer.Write((ushort)ChannelType);
        writer.Write(Pivot);
        writer.Write(Unknown);

        for (var i = 0; i < Data.Length; i++)
        {
            Data[i].WriteTo(writer, ChannelType);
        }

        for (var i = 0; i < NumPadBytes; i++)
        {
            writer.Write((byte)0);
        }
    }
}
