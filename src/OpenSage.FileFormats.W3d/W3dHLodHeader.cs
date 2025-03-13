using System.IO;

namespace OpenSage.FileFormats.W3d;

/// <param name="Version"></param>
/// <param name="LodCount"></param>
/// <param name="Name"></param>
/// <param name="HierarchyName">Name of the hierarchy tree to use</param>
public sealed record W3dHLodHeader(uint Version,
    uint LodCount,
    string Name,
    string HierarchyName) : W3dChunk(W3dChunkType.W3D_CHUNK_HLOD_HEADER)
{
    internal static W3dHLodHeader Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var version = reader.ReadUInt32();
            var lodCount = reader.ReadUInt32();
            var name = reader.ReadFixedLengthString(W3dConstants.NameLength);
            var hierarchyName = reader.ReadFixedLengthString(W3dConstants.NameLength);

            return new W3dHLodHeader(version, lodCount, name, hierarchyName);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        writer.Write(Version);
        writer.Write(LodCount);
        writer.WriteFixedLengthString(Name, W3dConstants.NameLength);
        writer.WriteFixedLengthString(HierarchyName, W3dConstants.NameLength);
    }
}
