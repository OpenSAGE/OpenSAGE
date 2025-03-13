using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dTriangles(W3dTriangle[] Items)
    : W3dStructListChunk<W3dTriangle>(W3dChunkType.W3D_CHUNK_TRIANGLES, Items)
{
    internal static W3dTriangles Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var items = ParseItems(header, reader, W3dTriangle.Parse);

            return new W3dTriangles(items);
        });
    }

    protected override void WriteItem(BinaryWriter writer, in W3dTriangle item)
    {
        item.WriteTo(writer);
    }
}
