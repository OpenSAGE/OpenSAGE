using System.IO;

namespace OpenSage.FileFormats.W3d
{
    public sealed class W3dAnimationChannel : W3dAnimationChannelBase
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_ANIMATION_CHANNEL;

        public ushort FirstFrame { get; private set; }

        public ushort LastFrame { get; private set; }

        /// <summary>
        /// Length of each vector in this channel.
        /// </summary>
        public ushort VectorLength { get; private set; }

        public W3dAnimationChannelType ChannelType { get; private set; }

        /// <summary>
        /// Pivot affected by this channel.
        /// </summary>
        public ushort Pivot { get; private set; }

        // Maybe padding?
        public ushort Unknown { get; private set; }

        public W3dAnimationChannelDatum[] Data { get; private set; }

        public uint NumPadBytes { get; private set; }

        internal static W3dAnimationChannel Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var startPosition = reader.BaseStream.Position;

                var result = new W3dAnimationChannel
                {
                    FirstFrame = reader.ReadUInt16(),
                    LastFrame = reader.ReadUInt16(),
                    VectorLength = reader.ReadUInt16(),
                    ChannelType = reader.ReadUInt16AsEnum<W3dAnimationChannelType>(),
                    Pivot = reader.ReadUInt16(),
                    Unknown = reader.ReadUInt16()
                };

                ValidateChannelDataSize(result.ChannelType, result.VectorLength);

                var numElements = result.LastFrame - result.FirstFrame + 1;
                var data = new W3dAnimationChannelDatum[numElements];

                for (var i = 0; i < numElements; i++)
                {
                    data[i] = W3dAnimationChannelDatum.Parse(reader, result.ChannelType);
                }

                result.Data = data;

                result.NumPadBytes = (uint) (context.CurrentEndPosition - reader.BaseStream.Position);
                reader.BaseStream.Seek((int) result.NumPadBytes, SeekOrigin.Current);

                return result;
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
            writer.Write((ushort) ChannelType);
            writer.Write(Pivot);
            writer.Write(Unknown);

            for (var i = 0; i < Data.Length; i++)
            {
                Data[i].WriteTo(writer, ChannelType);
            }

            for (var i = 0; i < NumPadBytes; i++)
            {
                writer.Write((byte) 0);
            }
        }
    }
}
