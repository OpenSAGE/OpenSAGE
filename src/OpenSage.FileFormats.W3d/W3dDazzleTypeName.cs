using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dDazzleTypeName(string TypeName) : W3dChunk(W3dChunkType.W3D_CHUNK_DAZZLE_TYPENAME)
{
    internal static W3dDazzleTypeName Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var typeName = reader.ReadFixedLengthString((int)context.CurrentEndPosition - (int)reader.BaseStream.Position);

            return new W3dDazzleTypeName(typeName);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        writer.WriteFixedLengthString(TypeName, TypeName.Length + 1);
    }
}
