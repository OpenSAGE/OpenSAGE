using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dSoundRObjHeader(uint Version, string Name) : W3dChunk(W3dChunkType.W3D_CHUNK_SOUNDROBJ_HEADER)
{
    internal static W3dSoundRObjHeader Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var version = reader.ReadUInt32();
            var name = reader.ReadFixedLengthString((int)header.ChunkSize - 4);

            return new W3dSoundRObjHeader(version, name);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        writer.Write(Version);
        writer.WriteFixedLengthString(Name, Name.Length + 1);
    }
}
