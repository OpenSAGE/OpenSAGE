using System.IO;
using System.Numerics;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public class W3dSpherePlaceholder
    {
        public uint ChunkType { get; private set; }

        public uint ChunkSize { get; private set; }

        internal static W3dSpherePlaceholder Parse(BinaryReader reader)
        {
            var result = new W3dSpherePlaceholder
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