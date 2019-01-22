namespace OpenSage.FileFormats.W3d
{
    public sealed class W3dPrelitVertex : W3dPrelitBase<W3dPrelitVertex>
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_PRELIT_VERTEX;
    }
}
