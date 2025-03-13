using System.Collections.Generic;
using System.IO;

namespace OpenSage.FileFormats.W3d;


public sealed record W3dCollection(
    W3dCollectionHeader Header,
    W3dCollectionObjName? ObjName,
    W3dCollectionPlaceholder? Placeholder,
    W3dCollectionTransformNode? TransformNode) : W3dContainerChunk(W3dChunkType.W3D_CHUNK_COLLECTION)
{
    internal static W3dCollection Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            W3dCollectionHeader? resultHeader = null;
            W3dCollectionObjName? objName = null;
            W3dCollectionPlaceholder? placeholder = null;
            W3dCollectionTransformNode? transformNode = null;

            ParseChunks(reader, context.CurrentEndPosition, chunkType =>
            {
                switch (chunkType)
                {
                    case W3dChunkType.W3D_CHUNK_COLLECTION_HEADER:
                        resultHeader = W3dCollectionHeader.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_COLLECTION_OBJ_NAME:
                        objName = W3dCollectionObjName.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_PLACEHOLDER:
                        placeholder = W3dCollectionPlaceholder.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_TRANSFORM_NODE:
                        transformNode = W3dCollectionTransformNode.Parse(reader, context);
                        break;
                    default:
                        throw CreateUnknownChunkException(chunkType);
                }
            });

            if (resultHeader is null)
            {
                throw new InvalidDataException("header should never be null");
            }

            return new W3dCollection(resultHeader, objName, placeholder, transformNode);
        });
    }

    protected override IEnumerable<W3dChunk> GetSubChunksOverride()
    {
        yield return Header;

        if (ObjName != null)
        {
            yield return ObjName;
        }

        if (Placeholder != null)
        {
            yield return Placeholder;
        }

        if (TransformNode != null)
        {
            yield return TransformNode;
        }
    }
}
