using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dDazzleName(string Name) : W3dChunk(W3dChunkType.W3D_CHUNK_DAZZLE_NAME)
{
    internal static W3dDazzleName Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var name = reader.ReadFixedLengthString((int)context.CurrentEndPosition - (int)reader.BaseStream.Position);

            return new W3dDazzleName(name);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        writer.WriteFixedLengthString(Name, Name.Length + 1);
    }
}
