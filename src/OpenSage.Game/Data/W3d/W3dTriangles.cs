using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dTriangles : W3dStructListChunk<W3dTriangles, W3dTriangle>
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_TRIANGLES;

        internal static W3dTriangles Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseList(reader, context, W3dTriangle.Parse);
        }

        protected override void WriteItem(BinaryWriter writer, in W3dTriangle item)
        {
            item.WriteTo(writer);
        }
    }
}
