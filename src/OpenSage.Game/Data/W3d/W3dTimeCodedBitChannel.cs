using OpenSage.Data.Utilities.Extensions;
using System.Diagnostics;
using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dTimeCodedBitChannel
    {
        public uint NumTimeCodes { get; private set; }

        /// <summary>
        /// Pivot affected by this channel.
        /// </summary>
        public ushort Pivot { get; private set; }

        public W3dBitChannelType ChannelType { get; private set; }

        public bool DefaultValue { get; private set; }

        public W3dTimeCodedBitDatum[] Data { get; private set; }

        public static W3dTimeCodedBitChannel Parse(BinaryReader reader, uint chunkSize)
        {
            var startPosition = reader.BaseStream.Position;

            var result = new W3dTimeCodedBitChannel
            {
                NumTimeCodes = reader.ReadUInt32(),
                Pivot = reader.ReadUInt16(),
                ChannelType = (W3dBitChannelType) reader.ReadByte(),
                DefaultValue = reader.ReadBooleanChecked()
            };

            var data = new W3dTimeCodedBitDatum[result.NumTimeCodes];

            for (var i = 0; i < result.NumTimeCodes; i++)
            {
                var timecode = reader.ReadUInt32();

                // TODO: Verify this guess.
                var value = false;
                if ((timecode >> 31) == 1)
                {
                    value = true;

                    timecode &= ~(1 << 31);
                }

                data[i] = new W3dTimeCodedBitDatum
                {
                    TimeCode = timecode,
                    Value = value
                };
            }

            result.Data = data;

            return result;
        }
    }

    [DebuggerDisplay("TimeCode = {TimeCode}, Value = {Value}")]
    public sealed class W3dTimeCodedBitDatum
    {
        public uint TimeCode;
        public bool Value;
    }
}
