using System.Collections.Generic;
using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dTextures(IReadOnlyList<W3dTexture> Items)
    : W3dListContainerChunk<W3dTexture>(W3dChunkType.W3D_CHUNK_TEXTURES, Items)
{
    internal static W3dTextures Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var items = ParseItems(reader, context, W3dChunkType.W3D_CHUNK_TEXTURE, W3dTexture.Parse);

            return new W3dTextures(items);
        });
    }
}
