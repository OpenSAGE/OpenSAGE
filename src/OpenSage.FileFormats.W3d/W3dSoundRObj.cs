using System.Collections.Generic;
using System.IO;

namespace OpenSage.FileFormats.W3d;


public sealed record W3dSoundRObj(W3dSoundRObjHeader Header, W3dSoundRObjDefinition? Definition)
    : W3dContainerChunk(W3dChunkType.W3D_CHUNK_SOUNDROBJ)
{
    internal static W3dSoundRObj Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            W3dSoundRObjHeader? resultHeader = null;
            W3dSoundRObjDefinition? definition = null;

            ParseChunks(reader, context.CurrentEndPosition, chunkType =>
            {
                switch (chunkType)
                {
                    case W3dChunkType.W3D_CHUNK_SOUNDROBJ_HEADER:
                        resultHeader = W3dSoundRObjHeader.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_SOUNDROBJ_DEFINITION:
                        definition = W3dSoundRObjDefinition.Parse(reader, context);
                        break;

                    default:
                        throw CreateUnknownChunkException(chunkType);
                }
            });

            if (resultHeader is null)
            {
                throw new InvalidDataException("header should never be null");
            }

            return new W3dSoundRObj(resultHeader, definition);
        });
    }

    protected override IEnumerable<W3dChunk> GetSubChunksOverride()
    {
        yield return Header;

        if (Definition != null)
        {
            yield return Definition;
        }
    }
}
