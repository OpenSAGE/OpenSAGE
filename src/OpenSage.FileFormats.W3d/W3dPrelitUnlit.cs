namespace OpenSage.FileFormats.W3d
{
    public sealed class W3dPrelitUnlit : W3dPrelitBase<W3dPrelitUnlit>
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_PRELIT_UNLIT;
    }
}
