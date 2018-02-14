using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Rep
{
    public sealed class ReplayChunkHeader
    {
        public uint Timecode { get; private set; }
        public GameMessageType MessageType { get; private set; }
        public uint Number { get; private set; }

        internal static ReplayChunkHeader Parse(BinaryReader reader)
        {
            return new ReplayChunkHeader
            {
                Timecode = reader.ReadUInt32(),
                MessageType = reader.ReadUInt32AsEnum<GameMessageType>(),
                Number = reader.ReadUInt32()
            };
        }
    }
}
