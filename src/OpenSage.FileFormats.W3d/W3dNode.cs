using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dNode(string RenderObjectName, uint PivotId) : W3dChunk(W3dChunkType.W3D_CHUNK_NODE)
{
    internal static W3dNode Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var renderObjectName = reader.ReadFixedLengthString(W3dConstants.NameLength);
            var pivotId = reader.ReadUInt16();

            return new W3dNode(renderObjectName, pivotId);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        writer.Write(RenderObjectName);
        writer.Write(PivotId);
    }
}
