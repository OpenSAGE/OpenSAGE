using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dTextureName(string Value) : W3dChunk(W3dChunkType.W3D_CHUNK_TEXTURE_NAME)
{
    internal static W3dTextureName Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var value = reader.ReadFixedLengthString((int)header.ChunkSize);

            return new W3dTextureName(value);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        writer.WriteFixedLengthString(Value, Value.Length + 1);
    }
}
