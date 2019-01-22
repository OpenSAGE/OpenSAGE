using System.Collections.Generic;
using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dHModel : W3dContainerChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_HMODEL;

        public W3dHModelHeader Header { get; private set; }

        public W3dNode Node { get; private set; }

        public W3dCollisionNode CollisionNode { get; private set; }

        public W3dSkinNode SkinNode { get; private set; }

        public W3dHModelAuxData AuxData { get; private set; }

        public W3dShadowNode ShadowNode { get; private set; }

        internal static W3dHModel Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new W3dHModel();

                ParseChunks(reader, context.CurrentEndPosition, chunkType =>
                {
                    switch (chunkType)
                    {
                        case W3dChunkType.W3D_CHUNK_HMODEL_HEADER:
                            result.Header = W3dHModelHeader.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_NODE:
                            result.Node = W3dNode.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_COLLISION_NODE:
                            result.CollisionNode = W3dCollisionNode.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_SKIN_NODE:
                            result.SkinNode = W3dSkinNode.Parse(reader, context);
                            break;

                        case W3dChunkType.OBSOLETE_W3D_CHUNK_HMODEL_AUX_DATA:
                            result.AuxData = W3dHModelAuxData.Parse(reader, context);
                            break;

                        case W3dChunkType.OBSOLETE_W3D_CHUNK_SHADOW_NODE:
                            result.ShadowNode = W3dShadowNode.Parse(reader, context);
                            break;

                        default:
                            throw CreateUnknownChunkException(chunkType);
                    }
                });

                return result;
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
}
