using System.IO;

namespace OpenSage.FileFormats.W3d
{
    public sealed class W3dPivots : W3dListChunk<W3dPivots, W3dPivot>
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_PIVOTS;

        internal static W3dPivots Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseList(reader, context, W3dPivot.Parse);
        }

        protected override void WriteItem(BinaryWriter writer, W3dPivot item)
        {
            item.WriteTo(writer);
        }
    }
}
