using System.Collections.Generic;
using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dHModel(
    W3dHModelHeader Header,
    W3dNode? Node,
    W3dCollisionNode? CollisionNode,
    W3dSkinNode? SkinNode,
    W3dHModelAuxData? AuxData,
    W3dShadowNode? ShadowNode) : W3dContainerChunk(W3dChunkType.W3D_CHUNK_HMODEL)
{
    internal static W3dHModel Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            W3dHModelHeader? resultHeader = null;
            W3dNode? node = null;
            W3dCollisionNode? collisionNode = null;
            W3dSkinNode? skinNode = null;
            W3dHModelAuxData? auxData = null;
            W3dShadowNode? shadowNode = null;

            ParseChunks(reader, context.CurrentEndPosition, chunkType =>
            {
                switch (chunkType)
                {
                    case W3dChunkType.W3D_CHUNK_HMODEL_HEADER:
                        resultHeader = W3dHModelHeader.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_NODE:
                        node = W3dNode.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_COLLISION_NODE:
                        collisionNode = W3dCollisionNode.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_SKIN_NODE:
                        skinNode = W3dSkinNode.Parse(reader, context);
                        break;

                    case W3dChunkType.OBSOLETE_W3D_CHUNK_HMODEL_AUX_DATA:
                        auxData = W3dHModelAuxData.Parse(reader, context);
                        break;

                    case W3dChunkType.OBSOLETE_W3D_CHUNK_SHADOW_NODE:
                        shadowNode = W3dShadowNode.Parse(reader, context);
                        break;

                    default:
                        throw CreateUnknownChunkException(chunkType);
                }
            });

            if (resultHeader is null)
            {
                throw new InvalidDataException("header should never be null");
            }

            return new W3dHModel(resultHeader, node, collisionNode, skinNode, auxData, shadowNode);
        });
    }

    protected override IEnumerable<W3dChunk> GetSubChunksOverride()
    {
        yield return Header;

        if (Node != null)
        {
            yield return Node;
        }

        if (CollisionNode != null)
        {
            yield return CollisionNode;
        }

        if (SkinNode != null)
        {
            yield return SkinNode;
        }

        if (AuxData != null)
        {
            yield return AuxData;
        }

        if (ShadowNode != null)
        {
            yield return ShadowNode;
        }
    }
}
