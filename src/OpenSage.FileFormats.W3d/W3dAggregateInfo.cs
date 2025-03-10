using System.Collections.Generic;
using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dAggregateInfo(
    string BaseModelName,
    uint SubObjectCount,
    IReadOnlyList<W3dAggregateSubObject> SubObjects) : W3dChunk(W3dChunkType.W3D_CHUNK_AGGREGATE_INFO)
{
    internal static W3dAggregateInfo Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var baseModelName = reader.ReadFixedLengthString(W3dConstants.NameLength * 2);
            var subObjectCount = reader.ReadUInt32();

            var subObjects = new List<W3dAggregateSubObject>();
            for (var i = 0; i < subObjectCount; i++)
            {
                subObjects.Add(W3dAggregateSubObject.Parse(reader));
            }

            return new W3dAggregateInfo(baseModelName, subObjectCount, subObjects);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        writer.WriteFixedLengthString(BaseModelName, W3dConstants.NameLength * 2);
        writer.Write(SubObjectCount);
        foreach (var subObject in SubObjects)
        {
            subObject.WriteTo(writer);
        }
    }
}
