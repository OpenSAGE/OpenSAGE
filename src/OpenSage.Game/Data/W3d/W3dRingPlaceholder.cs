using System.IO;
using System.Numerics;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public class W3dRingPlaceholder
    {
        public uint ChunkType { get; private set; }

        public uint ChunkSize { get; private set; }

        internal static W3dRingPlaceholder Parse(BinaryReader reader)
        {
            var result = new W3dRingPlaceholder
            {
                ChunkType = reader.ReadUInt32(),
                ChunkSize = reader.ReadUInt32() & 0x7FFFFFFF
            };

            return result;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(ChunkType);
            writer.Write(ChunkSize);
        }
    }
}