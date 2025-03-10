using System.IO;
using OpenSage.Mathematics;

namespace OpenSage.FileFormats.W3d;

/// <param name="Attributes"></param>
/// <param name="Stage0Mapping"></param>
/// <param name="Stage1Mapping"></param>
/// <param name="Ambient"></param>
/// <param name="Diffuse"></param>
/// <param name="Specular"></param>
/// <param name="Emissive"></param>
/// <param name="Shininess">how tight the specular highlight will be, 1 - 1000 (default = 1)</param>
/// <param name="Opacity">how opaque the material is, 0.0 = invisible, 1.0 = fully opaque (default = 1)</param>
/// <param name="Translucency">how much light passes through the material. (default = 0)</param>
public sealed record W3dVertexMaterialInfo(
    W3dVertexMaterialFlags Attributes,
    W3dVertexMappingType Stage0Mapping,
    W3dVertexMappingType Stage1Mapping,
    ColorRgb Ambient,
    ColorRgb Diffuse,
    ColorRgb Specular,
    ColorRgb Emissive,
    float Shininess,
    float Opacity,
    float Translucency) : W3dChunk(W3dChunkType.W3D_CHUNK_MATERIAL_INFO)
{
    internal static W3dVertexMaterialInfo Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var rawAttributes = reader.ReadUInt32();

            var attributes = (W3dVertexMaterialFlags)(rawAttributes & 0xF);
            var stage0Mapping = ConvertStageMapping(rawAttributes, 0x00FF0000, 16);
            var stage1Mapping = ConvertStageMapping(rawAttributes, 0x0000FF00, 8);
            var ambient = reader.ReadColorRgb(true);
            var diffuse = reader.ReadColorRgb(true);
            var specular = reader.ReadColorRgb(true);
            var emissive = reader.ReadColorRgb(true);
            var shininess = reader.ReadSingle();
            var opacity = reader.ReadSingle();
            var translucency = reader.ReadSingle();

            return new W3dVertexMaterialInfo(attributes, stage0Mapping, stage1Mapping, ambient, diffuse, specular,
                emissive, shininess, opacity, translucency);
        });
    }

    private static W3dVertexMappingType ConvertStageMapping(uint attributes, uint mask, int shift)
    {
        return EnumUtility.CastValueAsEnum<uint, W3dVertexMappingType>((attributes & mask) >> shift);
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        var rawAttributes = (uint)Attributes;
        rawAttributes |= (uint)Stage0Mapping << 16;
        rawAttributes |= (uint)Stage1Mapping << 8;
        writer.Write(rawAttributes);

        writer.Write(Ambient, true);
        writer.Write(Diffuse, true);
        writer.Write(Specular, true);
        writer.Write(Emissive, true);

        writer.Write(Shininess);
        writer.Write(Opacity);
        writer.Write(Translucency);
    }
}
