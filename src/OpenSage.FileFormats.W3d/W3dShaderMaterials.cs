using System.IO;

namespace OpenSage.FileFormats.W3d
{
    public sealed class W3dShaderMaterials : W3dListContainerChunk<W3dShaderMaterials, W3dShaderMaterial>
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_SHADER_MATERIALS;

        protected override W3dChunkType ItemType { get; } = W3dChunkType.W3D_CHUNK_SHADER_MATERIAL;

        internal static W3dShaderMaterials Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseList(reader, context, W3dShaderMaterial.Parse);
        }
    }
}
