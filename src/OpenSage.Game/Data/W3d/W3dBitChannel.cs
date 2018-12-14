using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dBitChannel : W3dAnimationChannelBase
    {
        internal override W3dChunkType ChunkType => W3dChunkType.W3D_CHUNK_BIT_CHANNEL;

        public ushort FirstFrame { get; private set; }

        public ushort LastFrame { get; private set; }

        public W3dBitChannelType ChannelType { get; private set; }

        /// <summary>
        /// Pivot affected by this channel.
        /// </summary>
        public ushort Pivot { get; private set; }

        public bool DefaultValue { get; private set; }

        public bool[] Data { get; private set; }

        internal static W3dBitChannel Parse(BinaryReader reader)
        {
            var result = new W3dBitChannel
            {
                FirstFrame = reader.ReadUInt16(),
                LastFrame = reader.ReadUInt16(),
                ChannelType = reader.ReadUInt16AsEnum<W3dBitChannelType>(),
                Pivot = reader.ReadUInt16(),
                DefaultValue = reader.ReadBooleanChecked()
            };

            var numElements = result.LastFrame - result.FirstFrame + 1;
            result.Data = reader.ReadSingleBitBooleanArray((uint) numElements);

            return result;
        }

        internal override void WriteTo(BinaryWriter writer)
        {
            writer.Write(FirstFrame);
            writer.Write(LastFrame);
            writer.Write((ushort) ChannelType);
            writer.Write(Pivot);
            writer.Write(DefaultValue);
            writer.WriteSingleBitBooleanArray(Data);
        }
    }
}
