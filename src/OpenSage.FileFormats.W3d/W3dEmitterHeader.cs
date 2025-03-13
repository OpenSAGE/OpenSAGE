using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dEmitterHeader(uint Version, string Name) : W3dChunk(W3dChunkType.W3D_CHUNK_EMITTER_HEADER)
{
    internal static W3dEmitterHeader Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var version = reader.ReadUInt32();
            var name = reader.ReadFixedLengthString(W3dConstants.NameLength);

            return new W3dEmitterHeader(version, name);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        writer.Write(Version);
        writer.WriteFixedLengthString(Name, W3dConstants.NameLength);
    }
}
