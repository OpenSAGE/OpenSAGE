using System.Collections.Generic;
using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dShaders(IReadOnlyList<W3dShader> Items)
    : W3dListChunk<W3dShader>(W3dChunkType.W3D_CHUNK_SHADERS, Items)
{
    internal static W3dShaders Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var items = ParseItems(reader, context, W3dShader.Parse);

            return new W3dShaders(items);
        });
    }

    protected override void WriteItem(BinaryWriter writer, W3dShader item)
    {
        item.WriteTo(writer);
    }
}
