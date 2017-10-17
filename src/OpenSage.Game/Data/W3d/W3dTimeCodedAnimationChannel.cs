using System.Diagnostics;
using System.IO;

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

        public static W3dTimeCodedAnimationChannel Parse(BinaryReader reader, uint chunkSize)
        {
            var startPosition = reader.BaseStream.Position;

            var result = new W3dTimeCodedAnimationChannel
            {
                NumTimeCodes = reader.ReadUInt32(),
                Pivot = reader.ReadUInt16(),
                VectorLength = reader.ReadByte(),
                ChannelType = (W3dAnimationChannelType) reader.ReadByte()
            };

            W3dAnimationChannel.ValidateChannelDataSize(result.ChannelType, result.VectorLength);

            var data = new W3dTimeCodedDatum[result.NumTimeCodes];

            for (var i = 0; i < result.NumTimeCodes; i++)
            {
                var datum = new W3dTimeCodedDatum();

                datum.TimeCode = reader.ReadUInt32();

                // MSB is used to indicate a binary (non interpolated) movement
                if ((datum.TimeCode >> 31) == 1)
                {
                    // TODO: non-interpolated movement.

                    datum.TimeCode &= ~(1 << 31);
                }

                datum.Value = W3dAnimationChannelDatum.Parse(reader, result.ChannelType);

                data[i] = datum;
            }

            result.Data = data;

            return result;
        }
    }
    
    [DebuggerDisplay("TimeCode = {TimeCode}, Value = {Value}")]
    public sealed class W3dTimeCodedDatum
    {
        public uint TimeCode;
        public W3dAnimationChannelDatum Value;
    }
}
