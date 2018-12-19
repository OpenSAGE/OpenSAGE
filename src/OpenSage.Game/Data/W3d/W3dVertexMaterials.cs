using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dVertexMaterials : W3dListContainerChunk<W3dVertexMaterials, W3dVertexMaterial>
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_VERTEX_MATERIALS;

        protected override W3dChunkType ItemType { get; } = W3dChunkType.W3D_CHUNK_VERTEX_MATERIAL;

        internal static W3dVertexMaterials Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseList(reader, context, W3dVertexMaterial.Parse);
        }
    }
}
