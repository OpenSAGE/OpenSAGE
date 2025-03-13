using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dAggregateClassInfo(uint OriginalClassId, uint Flags, byte[] UnknownBytes)
    : W3dChunk(W3dChunkType.W3D_CHUNK_AGGREGATE_INFO)
{
    internal static W3dAggregateClassInfo Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var originalClassId = reader.ReadUInt32();
            var flags = reader.ReadUInt32();
            var unknownBytes = reader.ReadBytes((int)context.CurrentEndPosition - (int)reader.BaseStream.Position);

            // TODO: Determine what the flags do/are.
            // TODO: Determine W3dAggregateClassInfo UnknownBytes.

            return new W3dAggregateClassInfo(originalClassId, flags, unknownBytes);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        writer.Write(OriginalClassId);
        writer.Write(Flags);
        writer.Write(UnknownBytes);
    }
}
