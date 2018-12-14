using System.Diagnostics;
using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dTimeCodedAnimationChannel
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

        public W3dTimeCodedDatum[] Data { get; private set; }

        internal static W3dTimeCodedAnimationChannel Parse(BinaryReader reader)
        {
            var startPosition = reader.BaseStream.Position;

            var result = new W3dTimeCodedAnimationChannel
            {
                NumTimeCodes = reader.ReadUInt32(),
                Pivot = reader.ReadUInt16(),
                VectorLength = reader.ReadByte(),
                ChannelType = reader.ReadByteAsEnum<W3dAnimationChannelType>()
            };

            W3dAnimationChannel.ValidateChannelDataSize(result.ChannelType, result.VectorLength);

            var data = new W3dTimeCodedDatum[result.NumTimeCodes];
            for (var i = 0; i < result.NumTimeCodes; i++)
            {
                data[i] = W3dTimeCodedDatum.Parse(reader, result.ChannelType);
            }

            result.Data = data;

            return result;
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.Write(NumTimeCodes);
            writer.Write(Pivot);
            writer.Write(VectorLength);
            writer.Write((byte) ChannelType);

            for (var i = 0; i < Data.Length; i++)
            {
                Data[i].WriteTo(writer, ChannelType);
            }
        }
    }
    
    [DebuggerDisplay("TimeCode = {TimeCode}, Value = {Value}")]
    public sealed class W3dTimeCodedDatum
    {
        public uint TimeCode;
        public bool NonInterpolatedMovement;
        public W3dAnimationChannelDatum Value;

        internal static W3dTimeCodedDatum Parse(BinaryReader reader, W3dAnimationChannelType channelType)
        {
            var result = new W3dTimeCodedDatum();

            result.TimeCode = reader.ReadUInt32();

            // MSB is used to indicate a binary (non interpolated) movement
            if ((result.TimeCode >> 31) == 1)
            {
                result.NonInterpolatedMovement = true;
                // TODO: non-interpolated movement.

                result.TimeCode &= ~(1 << 31);
            }

            result.Value = W3dAnimationChannelDatum.Parse(reader, channelType);

            return result;
        }

        internal void WriteTo(BinaryWriter writer, W3dAnimationChannelType channelType)
        {
            var timeCode = TimeCode;
            if (NonInterpolatedMovement)
            {
                timeCode |= (1u << 31);
            }
            writer.Write(timeCode);

            Value.WriteTo(writer, channelType);
        }
    }
}
