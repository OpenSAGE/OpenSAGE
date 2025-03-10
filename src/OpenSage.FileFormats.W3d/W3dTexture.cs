using System.Collections.Generic;
using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dTexture(W3dTextureName Name, W3dTextureInfo? TextureInfo)
    : W3dContainerChunk(W3dChunkType.W3D_CHUNK_TEXTURE)
{
    internal static W3dTexture Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            W3dTextureName? name = null;
            W3dTextureInfo? textureInfo = null;

            ParseChunks(reader, context.CurrentEndPosition, chunkType =>
            {
                switch (chunkType)
                {
                    case W3dChunkType.W3D_CHUNK_TEXTURE_NAME:
                        name = W3dTextureName.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_TEXTURE_INFO:
                        textureInfo = W3dTextureInfo.Parse(reader, context);
                        break;

                    default:
                        throw CreateUnknownChunkException(chunkType);
                }
            });

            if (name is null)
            {
                throw new InvalidDataException("name should never be null");
            }

            return new W3dTexture(name, textureInfo);
        });
    }

    protected override IEnumerable<W3dChunk> GetSubChunksOverride()
    {
        yield return Name;

        if (TextureInfo != null)
        {
            yield return TextureInfo;
        }
    }
}
