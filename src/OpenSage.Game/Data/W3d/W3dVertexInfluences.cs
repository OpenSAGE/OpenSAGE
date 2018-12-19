using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dVertexInfluences : W3dStructListChunk<W3dVertexInfluences, W3dVertexInfluence>
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_VERTEX_INFLUENCES;

        internal static W3dVertexInfluences Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseList(reader, context, W3dVertexInfluence.Parse);
        }

        protected override void WriteItem(BinaryWriter writer, in W3dVertexInfluence item)
        {
            item.WriteTo(writer);
        }
    }
}
