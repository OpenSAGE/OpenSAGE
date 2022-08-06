using System.Collections.Generic;
using System.IO;

namespace OpenSage.FileFormats.W3d
{

    public sealed class W3dCollection : W3dContainerChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_COLLECTION;

        public W3dCollectionHeader Header { get; private set; }

        public W3dCollectionObjName ObjName { get; private set; }

        public W3dCollectionPlaceholder Placeholder { get; private set; }

        public W3dCollectionTransformNode TransformNode { get; private set; }

        internal static W3dCollection Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new W3dCollection();

                ParseChunks(reader, context.CurrentEndPosition, chunkType =>
                {
                    switch (chunkType)
                    {
                        case W3dChunkType.W3D_CHUNK_COLLECTION_HEADER:
                            result.Header = W3dCollectionHeader.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_COLLECTION_OBJ_NAME:
                            result.ObjName = W3dCollectionObjName.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_PLACEHOLDER:
                            result.Placeholder = W3dCollectionPlaceholder.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_TRANSFORM_NODE:
                            result.TransformNode = W3dCollectionTransformNode.Parse(reader, context);
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
}
