using System.Collections.Generic;
using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dShadersPs2(IReadOnlyList<W3dShaderPs2> Items)
    : W3dListChunk<W3dShaderPs2>(W3dChunkType.W3D_CHUNK_PS2_SHADERS, Items)
{
    internal static W3dShadersPs2 Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var items = ParseItems(reader, context, W3dShaderPs2.Parse);

            return new W3dShadersPs2(items);
        });
    }

    protected override void WriteItem(BinaryWriter writer, W3dShaderPs2 item)
    {
        item.WriteTo(writer);
    }
}
