using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dPrelitUnlit(
    W3dMaterialInfo MaterialInfo,
    W3dVertexMaterials? VertexMaterials,
    W3dShaders? Shaders,
    W3dTextures? Textures,
    W3dMaterialPass? MaterialPass1,
    W3dMaterialPass? MaterialPass2) : W3dPrelitBase(W3dChunkType.W3D_CHUNK_PRELIT_UNLIT, MaterialInfo, VertexMaterials,
    Shaders, Textures, MaterialPass1, MaterialPass2)
{
    internal static W3dPrelitUnlit Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var (w3dMaterialInfo, w3dVertexMaterials, w3dShaders, w3dTextures, w3dMaterialPass, materialPass2) = ParseInternal(reader, context);

            return new W3dPrelitUnlit(w3dMaterialInfo, w3dVertexMaterials, w3dShaders, w3dTextures, w3dMaterialPass, materialPass2);
        });
    }
}
