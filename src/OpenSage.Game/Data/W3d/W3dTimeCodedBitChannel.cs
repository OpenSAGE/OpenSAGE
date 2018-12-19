using OpenSage.Data.Utilities.Extensions;
using System.Diagnostics;
using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dTimeCodedBitChannel : W3dChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_COMPRESSED_BIT_CHANNEL;

        public uint NumTimeCodes { get; private set; }

        /// <summary>
        /// Pivot affected by this channel.
        /// </summary>
        public ushort Pivot { get; private set; }

        public W3dBitChannelType ChannelType { get; private set; }

        public bool DefaultValue { get; private set; }

        public W3dTimeCodedBitDatum[] Data { get; private set; }

        internal static W3dTimeCodedBitChannel Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new W3dTimeCodedBitChannel
                {
                    NumTimeCodes = reader.ReadUInt32(),
                    Pivot = reader.ReadUInt16(),
                    ChannelType = reader.ReadByteAsEnum<W3dBitChannelType>(),
                    DefaultValue = reader.ReadBooleanChecked()
                };

                var data = new W3dTimeCodedBitDatum[result.NumTimeCodes];

                for (var i = 0; i < result.NumTimeCodes; i++)
                {
                    data[i] = W3dTimeCodedBitDatum.Parse(reader);
                }

                result.Data = data;

                return result;
            });
        }

        protected override void WriteToOverride(BinaryWriter writer)
        {
            writer.Write(NumTimeCodes);
            writer.Write(Pivot);
            writer.Write((byte) ChannelType);
            writer.Write(DefaultValue);

            for (var i = 0; i < Data.Length; i++)
            {
                Data[i].WriteTo(writer);
            }
        }
    }

    [DebuggerDisplay("TimeCode = {TimeCode}, Value = {Value}")]
    public sealed class W3dTimeCodedBitDatum
    {
        public uint TimeCode { get; private set; }
        public bool Value { get; private set; }

        internal static W3dTimeCodedBitDatum Parse(BinaryReader reader)
        {
            var timecode = reader.ReadUInt32();

            // TODO: Verify this guess.
            var value = false;
            if ((timecode >> 31) == 1)
            {
                value = true;

                timecode &= ~(1 << 31);
            }

            return new W3dTimeCodedBitDatum
            {
                TimeCode = timecode,
                Value = value
            };
        }

        internal void WriteTo(BinaryWriter writer)
        {
            var valueToWrite = TimeCode;

            if (Value)
            {
                valueToWrite |= (1u << 31);
            }

            writer.Write(valueToWrite);
        }
    }
}
