using System.IO;
using OpenSage.Data.Utilities;

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

        public W3dTimeCodedBitDatum[] Data { get; private set; }

        public static W3dAdaptiveDeltaAnimationChannel Parse(BinaryReader reader, uint chunkSize)
        {
            var startPosition = reader.BaseStream.Position;

            var result = new W3dAdaptiveDeltaAnimationChannel
            {
                NumTimeCodes = reader.ReadUInt32(),
                Pivot = reader.ReadUInt16(),
                VectorLength = reader.ReadByte(),
                ChannelType = EnumUtility.CastValueAsEnum<byte, W3dAnimationChannelType>(reader.ReadByte()),
                Scale = reader.ReadSingle()
            };

            W3dAnimationChannel.ValidateChannelDataSize(result.ChannelType, result.VectorLength);

            // TODO
            reader.ReadBytes((int)(startPosition + chunkSize - reader.BaseStream.Position));

            return result;
        }
    }
}
