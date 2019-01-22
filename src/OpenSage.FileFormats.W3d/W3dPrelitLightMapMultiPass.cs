namespace OpenSage.FileFormats.W3d
{
    public sealed class W3dPrelitLightMapMultiPass : W3dPrelitBase<W3dPrelitLightMapMultiPass>
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_PRELIT_LIGHTMAP_MULTI_PASS;
    }
}
