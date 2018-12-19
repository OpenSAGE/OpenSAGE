using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dTextures : W3dListContainerChunk<W3dTextures, W3dTexture>
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_TEXTURES;

        protected override W3dChunkType ItemType { get; } = W3dChunkType.W3D_CHUNK_TEXTURE;

        internal static W3dTextures Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseList(reader, context, W3dTexture.Parse);
        }
    }
}
