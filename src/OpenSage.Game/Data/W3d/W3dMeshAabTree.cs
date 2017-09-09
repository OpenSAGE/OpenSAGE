using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dMeshAabTree : W3dChunk
    {
        public W3dMeshAabTreeHeader Header { get; private set; }
        public uint[] PolygonIndices { get; private set; }
        public W3dMeshAabTreeNode[] Nodes { get; private set; }

        public static W3dMeshAabTree Parse(BinaryReader reader, uint chunkSize)
        {
            return ParseChunk<W3dMeshAabTree>(reader, chunkSize, (result, header) =>
            {
                switch (header.ChunkType)
                {
                    case W3dChunkType.W3D_CHUNK_AABTREE_HEADER:
                        result.Header = W3dMeshAabTreeHeader.Parse(reader);
                        break;

                    case W3dChunkType.W3D_CHUNK_AABTREE_POLYINDICES:
                        result.PolygonIndices = new uint[result.Header.PolyCount];
                        for (var i = 0; i < result.Header.PolyCount; i++)
                        {
                            result.PolygonIndices[i] = reader.ReadUInt32();
                        }
                        break;

                    case W3dChunkType.W3D_CHUNK_AABTREE_NODES:
                        result.Nodes = new W3dMeshAabTreeNode[result.Header.NodeCount];
                        for (var i = 0; i < result.Header.NodeCount; i++)
                        {
                            result.Nodes[i] = W3dMeshAabTreeNode.Parse(reader);
                        }
                        break;

                    default:
                        throw CreateUnknownChunkException(header);
                }
            });
        }
    }
}
