using System.Collections.Generic;
using System.IO;
using OpenSage.Mathematics;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dHierarchyDef(
    W3dHierarchy Header,
    W3dPivots Pivots,
    W3dPivotFixups? PivotFixups) : W3dContainerChunk(W3dChunkType.W3D_CHUNK_HIERARCHY)
{
    internal static W3dHierarchyDef Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            W3dHierarchy? resultHeader = null;
            W3dPivots? pivots = null;
            W3dPivotFixups? pivotFixups = null;

            ParseChunks(reader, context.CurrentEndPosition, chunkType =>
            {
                switch (chunkType)
                {
                    case W3dChunkType.W3D_CHUNK_HIERARCHY_HEADER:
                        resultHeader = W3dHierarchy.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_PIVOTS:
                        pivots = W3dPivots.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_PIVOT_FIXUPS:
                        pivotFixups = W3dPivotFixups.Parse(reader, context);
                        break;

                    default:
                        throw CreateUnknownChunkException(chunkType);
                }
            });

            if (resultHeader is null || pivots is null)
            {
                throw new InvalidDataException("header and pivots should never be null");
            }

            return new W3dHierarchyDef(resultHeader, pivots, pivotFixups);
        });
    }

    protected override IEnumerable<W3dChunk> GetSubChunksOverride()
    {
        yield return Header;
        yield return Pivots;

        if (PivotFixups != null)
        {
            yield return PivotFixups;
        }
    }
}

public sealed record W3dPivotFixups(Matrix4x3[] Items)
    : W3dStructListChunk<Matrix4x3>(W3dChunkType.W3D_CHUNK_PIVOT_FIXUPS, Items)
{
    internal static W3dPivotFixups Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var items = ParseItems(header, reader, r => r.ReadMatrix4x3());

            return new W3dPivotFixups(items);
        });
    }

    protected override void WriteItem(BinaryWriter writer, in Matrix4x3 item)
    {
        writer.Write(item);
    }
}
