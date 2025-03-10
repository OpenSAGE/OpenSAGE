using System.Collections.Generic;
using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dPivots(IReadOnlyList<W3dPivot> Items)
    : W3dListChunk<W3dPivot>(W3dChunkType.W3D_CHUNK_PIVOTS, Items)
{
    internal static W3dPivots Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var items = ParseItems(reader, context, W3dPivot.Parse);

            return new W3dPivots(items);
        });
    }

    protected override void WriteItem(BinaryWriter writer, W3dPivot item)
    {
        item.WriteTo(writer);
    }
}
