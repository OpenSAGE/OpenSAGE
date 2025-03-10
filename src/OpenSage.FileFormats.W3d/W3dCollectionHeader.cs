using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dCollectionHeader(uint Version, string Name, uint RenderObjectCount)
    : W3dChunk(W3dChunkType.W3D_CHUNK_COLLECTION_HEADER)
{
    internal static W3dCollectionHeader Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var version = reader.ReadUInt32();
            var name = reader.ReadFixedLengthString((int)context.CurrentEndPosition - (int)reader.BaseStream.Position - 4);
            var renderObjectCount = reader.ReadUInt32();

            return new W3dCollectionHeader(version, name, renderObjectCount);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        writer.Write(Version);
        writer.WriteFixedLengthString(Name, Name.Length + 1);
        writer.Write(RenderObjectCount);
    }
}
