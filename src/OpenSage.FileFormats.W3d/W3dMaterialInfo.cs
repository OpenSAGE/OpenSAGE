using System.IO;

namespace OpenSage.FileFormats.W3d;

/// <summary>
/// The VertexMaterial defines parameters which control the calculation of the primary
/// and secondary gradients. The shader defines how those gradients are combined with
/// the texel and the frame buffer contents.
/// </summary>
/// <param name="PassCount">How many material passes this render object uses</param>
/// <param name="VertexMaterialCount">How many vertex materials are used</param>
/// <param name="ShaderCount">How many shaders are used</param>
/// <param name="TextureCount">How many textures are used</param>
public sealed record W3dMaterialInfo(uint PassCount, uint VertexMaterialCount, uint ShaderCount, uint TextureCount)
    : W3dChunk(W3dChunkType.W3D_CHUNK_MATERIAL_INFO)
{
    internal static W3dMaterialInfo Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var passCount = reader.ReadUInt32();
            var vertexMaterialCount = reader.ReadUInt32();
            var shaderCount = reader.ReadUInt32();
            var textureCount = reader.ReadUInt32();

            return new W3dMaterialInfo(passCount, vertexMaterialCount, shaderCount, textureCount);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        writer.Write(PassCount);
        writer.Write(VertexMaterialCount);
        writer.Write(ShaderCount);
        writer.Write(TextureCount);
    }
}
