using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{

    public sealed class W3dAggregateUnknown : W3dChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_AGGREGATE_UNKNOWN;

        public byte[] UnknownBytes { get; private set; }

        internal static W3dAggregateUnknown Parse(BinaryReader reader, W3dParseContext context)
        {
            var result = new W3dAggregateUnknown
            {
                UnknownBytes = reader.ReadBytes((int)reader.BaseStream.Length - (int)reader.BaseStream.Position)
            };

            // TODO: Determine W3dAggregateUnknown UnknownBytes.

            return result;
        }
   
        protected override void WriteToOverride(BinaryWriter writer)
        {
            writer.Write(UnknownBytes);
        }
    }
}
