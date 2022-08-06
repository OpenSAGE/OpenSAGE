using System.IO;

namespace OpenSage.FileFormats.W3d
{
    public sealed class W3dAdaptiveDeltaAnimationChannel : W3dChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_COMPRESSED_ANIMATION_CHANNEL;

        public uint NumTimeCodes { get; private set; }

        /// <summary>
        /// Pivot affected by this channel.
        /// </summary>
        public ushort Pivot { get; private set; }

        /// <summary>
        /// Length of each vector in this channel.
        /// </summary>
        public byte VectorLength { get; private set; }

        public W3dAnimationChannelType ChannelType { get; private set; }

        /// <summary>
        ///  Filter table scale.
        /// </summary>
        public float Scale { get; private set; }

        public W3dAdaptiveDeltaData Data { get; private set; }

        internal static W3dAdaptiveDeltaAnimationChannel Parse(BinaryReader reader, W3dParseContext context, W3dAdaptiveDeltaBitCount bitCount)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new W3dAdaptiveDeltaAnimationChannel
                {
                    NumTimeCodes = reader.ReadUInt32(),
                    Pivot = reader.ReadUInt16(),
                    VectorLength = reader.ReadByte(),
                    ChannelType = reader.ReadByteAsEnum<W3dAnimationChannelType>(),
                    Scale = reader.ReadSingle(),
                };

                W3dAnimationChannel.ValidateChannelDataSize(result.ChannelType, result.VectorLength);

                result.Data = W3dAdaptiveDeltaData.Parse(
                    reader,
                    result.NumTimeCodes,
                    result.ChannelType,
                    result.VectorLength,
                    bitCount);

                // Skip 3 unknown bytes at chunk end.
                reader.BaseStream.Seek(3, SeekOrigin.Current);

                return result;
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
