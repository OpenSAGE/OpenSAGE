using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dVertexMaterialName : W3dChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_VERTEX_MATERIAL_NAME;

        public string Value { get; private set; }

        internal static W3dVertexMaterialName Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                return new W3dVertexMaterialName
                {
                    Value = reader.ReadFixedLengthString((int) header.ChunkSize)
                };
            });
        }

        protected override void WriteToOverride(BinaryWriter writer)
        {
            writer.WriteFixedLengthString(Value, Value.Length + 1);
        }
    }
}
