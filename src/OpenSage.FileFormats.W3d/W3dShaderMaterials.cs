using System.Collections.Generic;
using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dShaderMaterials(IReadOnlyList<W3dShaderMaterial> Items)
    : W3dListContainerChunk<W3dShaderMaterial>(W3dChunkType.W3D_CHUNK_SHADER_MATERIALS, Items)
{
    internal static W3dShaderMaterials Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var items = ParseItems(reader, context, W3dChunkType.W3D_CHUNK_SHADER_MATERIAL, W3dShaderMaterial.Parse);

            return new W3dShaderMaterials(items);
        });
    }
}
