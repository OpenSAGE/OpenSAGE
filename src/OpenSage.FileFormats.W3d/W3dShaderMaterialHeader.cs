using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dShaderMaterialHeader(byte Number, string TypeName)
    : W3dChunk(W3dChunkType.W3D_CHUNK_SHADER_MATERIAL_HEADER)
{
    internal static W3dShaderMaterialHeader Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var number = reader.ReadByte();
            var typeName = reader.ReadFixedLengthString(W3dConstants.NameLength * 2);

            reader.ReadUInt32(); // Reserved

            return new W3dShaderMaterialHeader(number, typeName);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        writer.Write(Number);
        writer.WriteFixedLengthString(TypeName, W3dConstants.NameLength * 2);

        writer.Write(0u); // Reserved
    }
}
