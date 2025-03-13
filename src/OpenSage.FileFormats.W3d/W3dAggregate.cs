using System.Collections.Generic;
using System.IO;

namespace OpenSage.FileFormats.W3d;

/// <summary>
/// An 'aggregate' is simply a 'shell' that contains references to a hierarchy model and subobjects to attach to its bones.
/// </summary>
public sealed record W3dAggregate(
    W3dAggregateHeader Header,
    W3dAggregateInfo? Info,
    W3dAggregateClassInfo? ClassInfo,
    W3dTextureReplacerInfo? TextureReplacerInfo) : W3dContainerChunk(W3dChunkType.W3D_CHUNK_AGGREGATE)
{
    internal static W3dAggregate Parse(BinaryReader reader, W3dParseContext context)
    {
        var parsedChunk = ParseChunk(reader, context, header =>
        {
            W3dAggregateHeader? resultHeader = null;
            W3dAggregateInfo? info = null;
            W3dAggregateClassInfo? classInfo = null;
            W3dTextureReplacerInfo? textureReplacerInfo = null;

            ParseChunks(reader, context.CurrentEndPosition, chunkType =>
            {
                switch (chunkType)
                {
                    case W3dChunkType.W3D_CHUNK_AGGREGATE_CLASS_INFO:
                        classInfo = W3dAggregateClassInfo.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_AGGREGATE_HEADER:
                        resultHeader = W3dAggregateHeader.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_AGGREGATE_INFO:
                        info = W3dAggregateInfo.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_TEXTURE_REPLACER_INFO:
                        textureReplacerInfo = W3dTextureReplacerInfo.Parse(reader, context);
                        break;

                    default:
                        throw CreateUnknownChunkException(chunkType);
                }
            });

            if (resultHeader is null)
            {
                throw new InvalidDataException("header should never be null");
            }

            return new W3dAggregate(resultHeader, info, classInfo, textureReplacerInfo);
        });

        return parsedChunk;
    }

    protected override IEnumerable<W3dChunk> GetSubChunksOverride()
    {
        yield return Header;

        if (Info != null)
        {
            yield return Info;
        }

        if (ClassInfo != null)
        {
            yield return ClassInfo;
        }

        if (TextureReplacerInfo != null)
        {
            yield return TextureReplacerInfo;
        }
    }
}
