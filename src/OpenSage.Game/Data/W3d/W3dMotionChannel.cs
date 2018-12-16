using OpenSage.Data.Utilities.Extensions;
using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dMotionChannel
    {
        /// <summary>
        /// Pivot affected by this channel.
        /// </summary>
        public ushort Pivot { get; private set; }

        public W3dMotionChannelDeltaType DeltaType { get; private set; }

        public byte VectorLength { get; private set; }

        public W3dAnimationChannelType ChannelType { get; private set; }

        public ushort NumTimeCodes { get; private set; }

        public W3dMotionChannelData Data { get; private set; }

        public static W3dMotionChannel Parse(BinaryReader reader, uint chunkSize)
        {
            var startPosition = reader.BaseStream.Position;

            var zero = reader.ReadByte();
            if (zero != 0)
            {
                throw new InvalidDataException();
            }

            var result = new W3dMotionChannel
            {
                DeltaType = reader.ReadByteAsEnum<W3dMotionChannelDeltaType>(),
                VectorLength = reader.ReadByte(),
                ChannelType = reader.ReadByteAsEnum<W3dAnimationChannelType>(),
                NumTimeCodes = reader.ReadUInt16(),
                Pivot = reader.ReadUInt16()
            };

            W3dAnimationChannel.ValidateChannelDataSize(result.ChannelType, result.VectorLength);

            switch (result.DeltaType)
            {
                case W3dMotionChannelDeltaType.TimeCoded:
                    result.Data = W3dMotionChannelTimeCodedData.Parse(reader, result.NumTimeCodes, result.ChannelType);
                    break;

                case W3dMotionChannelDeltaType.Delta4:
                    result.Data = W3dMotionChannelAdaptiveDeltaData.Parse(reader, result.NumTimeCodes, result.ChannelType, result.VectorLength, W3dAdaptiveDeltaBitCount.FourBits);
                    break;

                case W3dMotionChannelDeltaType.Delta8:
                    result.Data = W3dMotionChannelAdaptiveDeltaData.Parse(reader, result.NumTimeCodes, result.ChannelType, result.VectorLength, W3dAdaptiveDeltaBitCount.EightBits);
                    break;

                default:
                    throw new InvalidDataException();
            }

            return result;
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.Write((byte) DeltaType);
            writer.Write(VectorLength);
            writer.Write((byte) ChannelType);
            writer.Write(NumTimeCodes);
            writer.Write(Pivot);

            Data.WriteTo(writer, ChannelType);
        }
    }
}
