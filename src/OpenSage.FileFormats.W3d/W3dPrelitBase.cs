using System.Collections.Generic;
using System.IO;

namespace OpenSage.FileFormats.W3d;

public abstract record W3dPrelitBase(
    W3dChunkType ChunkType,
    W3dMaterialInfo MaterialInfo,
    W3dVertexMaterials? VertexMaterials,
    W3dShaders? Shaders,
    W3dTextures? Textures,
    W3dMaterialPass? MaterialPass1,
    W3dMaterialPass? MaterialPass2) : W3dContainerChunk(ChunkType)
{
    private protected static W3dPrelitContents ParseInternal(BinaryReader reader, W3dParseContext context)
    {
        W3dMaterialInfo? materialInfo = null;
        W3dVertexMaterials? vertexMaterials = null;
        W3dShaders? shaders = null;
        W3dTextures? textures = null;
        W3dMaterialPass? materialPass1 = null;
        W3dMaterialPass? materialPass2 = null;

        ParseChunks(reader, context.CurrentEndPosition, chunkType =>
        {
            switch (chunkType)
            {
                case W3dChunkType.W3D_CHUNK_MATERIAL_INFO:
                    materialInfo = W3dMaterialInfo.Parse(reader, context);
                    break;

                case W3dChunkType.W3D_CHUNK_VERTEX_MATERIALS:
                    vertexMaterials = W3dVertexMaterials.Parse(reader, context);
                    break;

                case W3dChunkType.W3D_CHUNK_SHADERS:
                    shaders = W3dShaders.Parse(reader, context);
                    break;

                case W3dChunkType.W3D_CHUNK_TEXTURES:
                    textures = W3dTextures.Parse(reader, context);
                    break;

                case W3dChunkType.W3D_CHUNK_MATERIAL_PASS:
                    if (materialPass1 == null)
                    {
                        materialPass1 = W3dMaterialPass.Parse(reader, context);
                    }
                    else
                    {
                        materialPass2 = W3dMaterialPass.Parse(reader, context);
                    }
                    break;

                default:
                    throw CreateUnknownChunkException(chunkType);
            }
        });

        if (materialInfo is null)
        {
            throw new InvalidDataException("materialInfo should never be null");
        }

        return new W3dPrelitContents(materialInfo, vertexMaterials, shaders, textures, materialPass1,
            materialPass2);
    }

    protected override IEnumerable<W3dChunk> GetSubChunksOverride()
    {
        yield return MaterialInfo;

        if (VertexMaterials != null)
        {
            yield return VertexMaterials;
        }

        if (Shaders != null)
        {
            yield return Shaders;
        }

        if (Textures != null)
        {
            yield return Textures;
        }

        if (MaterialPass1 != null)
        {
            yield return MaterialPass1;
        }

        if (MaterialPass2 != null)
        {
            yield return MaterialPass2;
        }
    }
}

internal record W3dPrelitContents(
    W3dMaterialInfo MaterialInfo,
    W3dVertexMaterials? VertexMaterials,
    W3dShaders? Shaders,
    W3dTextures? Textures,
    W3dMaterialPass? MaterialPass1,
    W3dMaterialPass? MaterialPass2);
