using System.Collections.Generic;
using System.IO;
using OpenSage.Mathematics;

namespace OpenSage.FileFormats.W3d;

/// <param name="VertexMaterialIds"></param>
/// <param name="ShaderIds"></param>
/// <param name="ShaderMaterialIds"></param>
/// <param name="Dcg">per-vertex diffuse color values</param>
/// <param name="Dig">per-vertex diffuse illumination values</param>
/// <param name="Scg">per-vertex specular color values</param>
/// <param name="TextureStages"></param>
/// <param name="TexCoords">Only present when using shader materials</param>
public sealed record W3dMaterialPass(
    W3dUInt32List? VertexMaterialIds,
    W3dUInt32List? ShaderIds,
    W3dUInt32List? ShaderMaterialIds,
    W3dRgbaList? Dcg,
    W3dRgbaList? Dig,
    W3dRgbaList? Scg,
    IReadOnlyList<W3dTextureStage> TextureStages,
    W3dVector2List? TexCoords) : W3dContainerChunk(W3dChunkType.W3D_CHUNK_MATERIAL_PASS)
{
    internal static W3dMaterialPass Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            W3dUInt32List? vertexMaterialIds = null;
            W3dUInt32List? shaderIds = null;
            W3dUInt32List? shaderMaterialIds = null;
            W3dRgbaList? dcg = null;
            W3dRgbaList? dig = null;
            W3dRgbaList? scg = null;
            List<W3dTextureStage> textureStages = [];
            W3dVector2List? texCoords = null;

            ParseChunks(reader, context.CurrentEndPosition, chunkType =>
            {
                switch (chunkType)
                {
                    case W3dChunkType.W3D_CHUNK_VERTEX_MATERIAL_IDS:
                        vertexMaterialIds = W3dUInt32List.Parse(reader, context, chunkType);
                        break;

                    case W3dChunkType.W3D_CHUNK_SHADER_IDS:
                        shaderIds = W3dUInt32List.Parse(reader, context, chunkType);
                        break;

                    case W3dChunkType.W3D_CHUNK_DCG:
                        dcg = W3dRgbaList.Parse(reader, context, chunkType);
                        break;

                    case W3dChunkType.W3D_CHUNK_DIG:
                        dig = W3dRgbaList.Parse(reader, context, chunkType);
                        break;

                    case W3dChunkType.W3D_CHUNK_SCG:
                        scg = W3dRgbaList.Parse(reader, context, chunkType);
                        break;

                    case W3dChunkType.W3D_CHUNK_TEXTURE_STAGE:
                        textureStages.Add(W3dTextureStage.Parse(reader, context));
                        break;

                    case W3dChunkType.W3D_CHUNK_SHADER_MATERIAL_ID:
                        shaderMaterialIds = W3dUInt32List.Parse(reader, context, chunkType);
                        break;

                    // Normally this appears inside W3dTextureStage, but it can also
                    // appear directly under W3dMaterialPass if using shader materials.
                    case W3dChunkType.W3D_CHUNK_STAGE_TEXCOORDS:
                        texCoords = W3dVector2List.Parse(reader, context, chunkType);
                        break;

                    default:
                        throw CreateUnknownChunkException(chunkType);
                }
            });

            return new W3dMaterialPass(vertexMaterialIds, shaderIds, shaderMaterialIds, dcg, dig, scg, textureStages,
                texCoords);
        });
    }

    protected override IEnumerable<W3dChunk> GetSubChunksOverride()
    {
        if (VertexMaterialIds != null)
        {
            yield return VertexMaterialIds;
        }

        if (ShaderIds != null)
        {
            yield return ShaderIds;
        }

        if (Dcg != null)
        {
            yield return Dcg;
        }

        if (Dig != null)
        {
            yield return Dig;
        }

        if (Scg != null)
        {
            yield return Scg;
        }

        foreach (var textureStage in TextureStages)
        {
            yield return textureStage;
        }

        if (ShaderMaterialIds != null)
        {
            yield return ShaderMaterialIds;
        }

        if (TexCoords != null)
        {
            yield return TexCoords;
        }
    }
}

public sealed record W3dRgbaList(W3dChunkType ChunkType, ColorRgba[] Items) : W3dStructListChunk<ColorRgba>(ChunkType, Items)
{
    internal static W3dRgbaList Parse(BinaryReader reader, W3dParseContext context, W3dChunkType chunkType)
    {
        return ParseChunk(reader, context, header =>
        {
            var items = ParseItems(header, reader, r => r.ReadColorRgba());

            return new W3dRgbaList(chunkType, items);
        });
    }

    protected override void WriteItem(BinaryWriter writer, in ColorRgba item)
    {
        writer.Write(item);
    }
}
