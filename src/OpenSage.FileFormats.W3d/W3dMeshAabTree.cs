using System.Collections.Generic;
using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dMeshAabTree : W3dContainerChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_AABTREE;

        public W3dMeshAabTreeHeader Header { get; private set; }
        public W3dMeshAabTreePolyIndices PolygonIndices { get; private set; }
        public W3dMeshAabTreeNodes Nodes { get; private set; }

        internal static W3dMeshAabTree Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk<W3dMeshAabTree>(reader, context, header =>
            {
                var result = new W3dMeshAabTree();
                ParseChunks(reader, context.CurrentEndPosition, chunkType =>
                {
                    switch (chunkType)
                    {
                        case W3dChunkType.W3D_CHUNK_AABTREE_HEADER:
                            result.Header = W3dMeshAabTreeHeader.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_AABTREE_POLYINDICES:
                            result.PolygonIndices = W3dMeshAabTreePolyIndices.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_AABTREE_NODES:
                            result.Nodes = W3dMeshAabTreeNodes.Parse(reader, context);
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
            yield return PolygonIndices;
            yield return Nodes;
        }
    }

    public sealed class W3dMeshAabTreePolyIndices : W3dStructListChunk<W3dMeshAabTreePolyIndices, uint>
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_AABTREE_POLYINDICES;

        internal static W3dMeshAabTreePolyIndices Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseList(reader, context, r => r.ReadUInt32());
        }

        protected override void WriteItem(BinaryWriter writer, in uint item)
        {
            writer.Write(item);
        }
    }
}
