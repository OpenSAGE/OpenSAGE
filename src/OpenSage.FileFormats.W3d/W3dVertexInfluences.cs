using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dVertexInfluences(W3dVertexInfluence[] Items)
    : W3dStructListChunk<W3dVertexInfluence>(W3dChunkType.W3D_CHUNK_VERTEX_INFLUENCES, Items)
{
    internal static W3dVertexInfluences Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var items = ParseItems(header, reader, W3dVertexInfluence.Parse);

            return new W3dVertexInfluences(items);
        });
    }

    protected override void WriteItem(BinaryWriter writer, in W3dVertexInfluence item)
    {
        item.WriteTo(writer);
    }
}
