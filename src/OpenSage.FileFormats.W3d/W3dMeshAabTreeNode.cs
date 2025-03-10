using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dMeshAabTreeNodes(IReadOnlyList<W3dMeshAabTreeNode> Items)
    : W3dListChunk<W3dMeshAabTreeNode>(W3dChunkType.W3D_CHUNK_AABTREE_NODES, Items)
{
    internal static W3dMeshAabTreeNodes Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var items = ParseItems(reader, context, W3dMeshAabTreeNode.Parse);

            return new W3dMeshAabTreeNodes(items);
        });
    }

    protected override void WriteItem(BinaryWriter writer, W3dMeshAabTreeNode item)
    {
        item.WriteTo(writer);
    }
}

/// <param name="Min">min corner of the box</param>
/// <param name="Max">max corner of the box</param>
/// <param name="FrontOrPoly0">index of the front child or poly0 (if MSB is set, then leaf and poly0 is valid)</param>
/// <param name="BackOrPolyCount">index of the back child or polycount</param>
public sealed record W3dMeshAabTreeNode(Vector3 Min, Vector3 Max, uint FrontOrPoly0, uint BackOrPolyCount)
{
    internal static W3dMeshAabTreeNode Parse(BinaryReader reader)
    {
        var min = reader.ReadVector3();
        var max = reader.ReadVector3();
        var frontOrPoly0 = reader.ReadUInt32();
        var backOrPolyCount = reader.ReadUInt32();

        return new W3dMeshAabTreeNode(min, max, frontOrPoly0, backOrPolyCount);
    }

    internal void WriteTo(BinaryWriter writer)
    {
        writer.Write(Min);
        writer.Write(Max);
        writer.Write(FrontOrPoly0);
        writer.Write(BackOrPolyCount);
    }
}
