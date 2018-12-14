using System.IO;
using OpenSage.Data.Utilities;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dAdaptiveDeltaAnimationChannel
    {
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

        public W3dAnimationChannelDatum[] Data { get; private set; }

        internal static W3dAdaptiveDeltaAnimationChannel Parse(BinaryReader reader, int nBits)
        {
            var startPosition = reader.BaseStream.Position;

            var result = new W3dAdaptiveDeltaAnimationChannel
            {
                NumTimeCodes = reader.ReadUInt32(),
                Pivot = reader.ReadUInt16(),
                VectorLength = reader.ReadByte(),
                ChannelType = reader.ReadByteAsEnum<W3dAnimationChannelType>(),
                Scale = reader.ReadSingle(),
            };

            W3dAnimationChannel.ValidateChannelDataSize(result.ChannelType, result.VectorLength);

            result.Data = W3dAdaptiveDelta.ReadAdaptiveDelta(
                reader,
                result.NumTimeCodes,
                result.ChannelType,
                result.VectorLength,
                result.Scale,
                nBits);

            //Skip 3 unknown bytes at chunkend. Only set for quaternions.
            reader.BaseStream.Seek(3, SeekOrigin.Current);

            return result;
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.Write(NumTimeCodes);
            writer.Write(Pivot);
            writer.Write(VectorLength);
            writer.Write((byte) ChannelType);
            writer.Write(Scale);

            // TODO
        }
    }
}
