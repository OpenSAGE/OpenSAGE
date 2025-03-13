using System.Collections.Generic;
using System.IO;

namespace OpenSage.FileFormats.W3d;

public record W3dHLodArray(W3dHLodArrayHeader Header, List<W3dHLodSubObject> SubObjects)
    : W3dContainerChunk(W3dChunkType.W3D_CHUNK_HLOD_LOD_ARRAY)
{
    internal static W3dHLodArray Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            W3dHLodArrayHeader? resultHeader = null;
            List<W3dHLodSubObject> subObjects = [];

            ParseChunks(reader, context.CurrentEndPosition, chunkType =>
            {
                switch (chunkType)
                {
                    case W3dChunkType.W3D_CHUNK_HLOD_SUB_OBJECT_ARRAY_HEADER:
                        resultHeader = W3dHLodArrayHeader.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_HLOD_SUB_OBJECT:
                        subObjects.Add(W3dHLodSubObject.Parse(reader, context));
                        break;

                    default:
                        throw CreateUnknownChunkException(chunkType);
                }
            });

            if (resultHeader is null)
            {
                throw new InvalidDataException("header should never be null");
            }

            return new W3dHLodArray(resultHeader, subObjects);
        });
    }

    protected override IEnumerable<W3dChunk> GetSubChunksOverride()
    {
        yield return Header;

        foreach (var subObject in SubObjects)
        {
            yield return subObject;
        }
    }
}
