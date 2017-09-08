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

            var data = new W3dTimeCodedDatum[result.NumTimeCodes];

            for (var i = 0; i < result.NumTimeCodes; i++)
            {
                var datum = new W3dTimeCodedDatum();

                datum.TimeCode = reader.ReadUInt32();

                // MSB is used to indicate a binary (non interpolated) movement
                if ((datum.TimeCode >> 31) == 1)
                {
                    throw new System.NotImplementedException();
                }

                datum.Values = new float[result.VectorLength];
                for (var j = 0; j < result.VectorLength; j++)
                {
                    datum.Values[j] = reader.ReadSingle();
                }

                data[i] = datum;
            }

            result.Data = data;

            return result;
        }
    }
    
    public sealed class W3dTimeCodedDatum
    {
        public uint TimeCode;
        public float[] Values;
    }
}
