using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dHModelHeader(
    uint Version,
    string Name,
    string HierarchyName,
    uint ConnectionsCount,
    byte[] UnknownBytes) : W3dChunk(W3dChunkType.W3D_CHUNK_HMODEL_HEADER)
{
    internal static W3dHModelHeader Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var version = reader.ReadUInt32();
            var name = reader.ReadFixedLengthString(W3dConstants.NameLength);
            var hierarchyName = reader.ReadFixedLengthString(W3dConstants.NameLength);
            var connectionsCount = reader.ReadUInt16();
            var unknownBytes = reader.ReadBytes((int)context.CurrentEndPosition - (int)reader.BaseStream.Position);

            // TODO: Determine W3dHModelHeader UnknownBytes

            return new W3dHModelHeader(version, name, hierarchyName, connectionsCount, unknownBytes);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        writer.Write(Version);
        writer.WriteFixedLengthString(Name, W3dConstants.NameLength);
        writer.Write(HierarchyName);
        writer.Write(ConnectionsCount);
        writer.Write(UnknownBytes);
    }
}
