using System.Collections.Generic;
using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dMeshAabTree(
    W3dMeshAabTreeHeader Header,
    W3dMeshAabTreePolyIndices PolygonIndices,
    W3dMeshAabTreeNodes Nodes) : W3dContainerChunk(W3dChunkType.W3D_CHUNK_AABTREE)
{
    internal static W3dMeshAabTree Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk<W3dMeshAabTree>(reader, context, header =>
        {
            W3dMeshAabTreeHeader? resultHeader = null;
            W3dMeshAabTreePolyIndices? polygonIndices = null;
            W3dMeshAabTreeNodes? nodes = null;

            ParseChunks(reader, context.CurrentEndPosition, chunkType =>
            {
                switch (chunkType)
                {
                    case W3dChunkType.W3D_CHUNK_AABTREE_HEADER:
                        resultHeader = W3dMeshAabTreeHeader.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_AABTREE_POLYINDICES:
                        polygonIndices = W3dMeshAabTreePolyIndices.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_AABTREE_NODES:
                        nodes = W3dMeshAabTreeNodes.Parse(reader, context);
                        break;

                    default:
                        throw CreateUnknownChunkException(chunkType);
                }
            });

            if (resultHeader is null || polygonIndices is null || nodes is null)
            {
                throw new InvalidDataException();
            }

            return new W3dMeshAabTree(resultHeader, polygonIndices, nodes);
        });
    }

    protected override IEnumerable<W3dChunk> GetSubChunksOverride()
    {
        yield return Header;
        yield return PolygonIndices;
        yield return Nodes;
    }
}

public sealed record W3dMeshAabTreePolyIndices(uint[] Items)
    : W3dStructListChunk<uint>(W3dChunkType.W3D_CHUNK_AABTREE_POLYINDICES, Items)
{
    internal static W3dMeshAabTreePolyIndices Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var items = ParseItems(header, reader, r => r.ReadUInt32());

            return new W3dMeshAabTreePolyIndices(items);
        });
    }

    protected override void WriteItem(BinaryWriter writer, in uint item)
    {
        writer.Write(item);
    }
}
