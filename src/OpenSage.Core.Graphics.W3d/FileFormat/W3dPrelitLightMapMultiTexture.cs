namespace OpenSage.FileFormats.W3d
{
    public sealed class W3dPrelitLightMapMultiTexture : W3dPrelitBase<W3dPrelitLightMapMultiTexture>
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_PRELIT_LIGHTMAP_MULTI_TEXTURE;
    }
}
