using System.Collections.Generic;
using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dVertexMaterials(IReadOnlyList<W3dVertexMaterial> Items)
    : W3dListContainerChunk<W3dVertexMaterial>(W3dChunkType.W3D_CHUNK_VERTEX_MATERIALS, Items)
{
    internal static W3dVertexMaterials Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var items = ParseItems(reader, context, W3dChunkType.W3D_CHUNK_VERTEX_MATERIAL, W3dVertexMaterial.Parse);

            return new W3dVertexMaterials(items);
        });
    }
}
