using System.IO;

namespace OpenSage.FileFormats.W3d
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="NumTimeCodes"></param>
    /// <param name="Pivot"></param>
    /// <param name="VectorLength">Length of each vector in this channel.</param>
    /// <param name="ChannelType"></param>
    /// <param name="Scale">Filter table scale.</param>
    /// <param name="Data"></param>
    public sealed record W3dAdaptiveDeltaAnimationChannel(
        uint NumTimeCodes,
        ushort Pivot,
        byte VectorLength,
        W3dAnimationChannelType ChannelType,
        float Scale,
        W3dAdaptiveDeltaData Data) : W3dChunk(W3dChunkType.W3D_CHUNK_COMPRESSED_ANIMATION_CHANNEL)
    {
        internal static W3dAdaptiveDeltaAnimationChannel Parse(BinaryReader reader, W3dParseContext context, W3dAdaptiveDeltaBitCount bitCount)
        {
            return ParseChunk(reader, context, header =>
            {
                var numTimeCodes = reader.ReadUInt32();
                var pivot = reader.ReadUInt16();
                var vectorLength = reader.ReadByte();
                var channelType = reader.ReadByteAsEnum<W3dAnimationChannelType>();
                var scale = reader.ReadSingle();

                W3dAnimationChannel.ValidateChannelDataSize(channelType, vectorLength);

                var data = W3dAdaptiveDeltaData.Parse(
                    reader,
                    numTimeCodes,
                    channelType,
                    vectorLength,
                    bitCount);

                // Skip 3 unknown bytes at chunk end.
                reader.BaseStream.Seek(3, SeekOrigin.Current);

                return new W3dAdaptiveDeltaAnimationChannel(numTimeCodes, pivot, vectorLength, channelType, scale, data);
            });
        }

        protected override void WriteToOverride(BinaryWriter writer)
        {
            writer.Write(NumTimeCodes);
            writer.Write(Pivot);
            writer.Write(VectorLength);
            writer.Write((byte) ChannelType);
            writer.Write(Scale);

            Data.WriteTo(writer, ChannelType);

            // Skip
            for (var i = 0; i < 3; i++)
            {
                writer.Write((byte) 0);
            }
        }
    }
}
