using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace OpenSage.FileFormats.W3d;

/// <param name="Header"></param>
/// <param name="UserText"></param>
/// <param name="Vertices"></param>
/// <param name="Normals"></param>
/// <param name="Vertices2">TODO: What are these? Some sort of alternate state?</param>
/// <param name="Normals2">TODO: What are these? Some sort of alternate state?</param>
/// <param name="Tangents"></param>
/// <param name="Bitangents"></param>
/// <param name="Influences">Vertex influences link vertices of a mesh to bones in a hierarchy. This is the information needed for skinning.</param>
/// <param name="Triangles"></param>
/// <param name="ShadeIndices">shade indexes for each vertex (array of uint32's)</param>
/// <param name="MaterialInfo"></param>
/// <param name="VertexMaterials"></param>
/// <param name="Shaders"></param>
/// <param name="ShadersPs2"></param>
/// <param name="Textures"></param>
/// <param name="MaterialPasses"></param>
/// <param name="AabTree"></param>
/// <param name="ShaderMaterials"></param>
/// <param name="PrelitVertex"></param>
/// <param name="PrelitLightMapMultiPass"></param>
/// <param name="PrelitLightMapMultiTexture"></param>
/// <param name="PrelitUnlit"></param>
public sealed record W3dMesh(
    W3dMeshHeader3 Header,
    W3dMeshUserText? UserText,
    W3dVector3List Vertices,
    W3dVector3List Normals,
    W3dVector3List? Vertices2,
    W3dVector3List? Normals2,
    W3dVector3List? Tangents,
    W3dVector3List? Bitangents,
    W3dVertexInfluences? Influences,
    W3dTriangles Triangles,
    W3dUInt32List ShadeIndices,
    W3dMaterialInfo MaterialInfo,
    W3dVertexMaterials? VertexMaterials,
    W3dShaders? Shaders,
    W3dShadersPs2? ShadersPs2,
    W3dTextures? Textures,
    IReadOnlyList<W3dMaterialPass> MaterialPasses,
    W3dMeshAabTree? AabTree,
    W3dShaderMaterials? ShaderMaterials,
    W3dPrelitVertex? PrelitVertex,
    W3dPrelitLightMapMultiPass? PrelitLightMapMultiPass,
    W3dPrelitLightMapMultiTexture? PrelitLightMapMultiTexture,
    W3dPrelitUnlit? PrelitUnlit) : W3dContainerChunk(W3dChunkType.W3D_CHUNK_MESH)
{
    public bool IsSkinned => (Header.Attributes & W3dMeshFlags.GeometryTypeMask) == W3dMeshFlags.GeometryTypeSkin;

    internal static W3dMesh Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            W3dMeshHeader3? resultHeader = null;
            W3dMeshUserText? userText = null;
            W3dVector3List? vertices = null;
            W3dVector3List? normals = null;
            W3dVector3List? vertices2 = null;
            W3dVector3List? normals2 = null;
            W3dVector3List? tangents = null;
            W3dVector3List? bitangents = null;
            W3dVertexInfluences? influences = null;
            W3dTriangles? triangles = null;
            W3dUInt32List? shadeIndices = null;
            W3dMaterialInfo? materialInfo = null;
            W3dVertexMaterials? vertexMaterials = null;
            W3dShaders? shaders = null;
            W3dShadersPs2? shadersPs2 = null;
            W3dTextures? textures = null;
            List<W3dMaterialPass> materialPasses = [];
            W3dMeshAabTree? aabTree = null;
            W3dShaderMaterials? shaderMaterials = null;
            W3dPrelitVertex? prelitVertex = null;
            W3dPrelitLightMapMultiPass? prelitLightMapMultiPass = null;
            W3dPrelitLightMapMultiTexture? prelitLightMapMultiTexture = null;
            W3dPrelitUnlit? prelitUnlit = null;

            ParseChunks(reader, context.CurrentEndPosition, chunkType =>
            {
                switch (chunkType)
                {
                    case W3dChunkType.W3D_CHUNK_MESH_HEADER3:
                        resultHeader = W3dMeshHeader3.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_VERTICES:
                        vertices = W3dVector3List.Parse(reader, context, chunkType);
                        break;

                    case W3dChunkType.W3D_CHUNK_VERTEX_NORMALS:
                        normals = W3dVector3List.Parse(reader, context, chunkType);
                        break;

                    case W3dChunkType.W3D_CHUNK_VERTEX_INFLUENCES:
                        influences = W3dVertexInfluences.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_TRIANGLES:
                        triangles = W3dTriangles.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_VERTEX_SHADE_INDICES:
                        shadeIndices = W3dUInt32List.Parse(reader, context, chunkType);
                        break;

                    case W3dChunkType.W3D_CHUNK_MATERIAL_INFO:
                        materialInfo = W3dMaterialInfo.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_VERTEX_MATERIALS:
                        vertexMaterials = W3dVertexMaterials.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_SHADERS:
                        shaders = W3dShaders.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_PS2_SHADERS:
                        shadersPs2 = W3dShadersPs2.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_TEXTURES:
                        textures = W3dTextures.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_MATERIAL_PASS:
                        materialPasses.Add(W3dMaterialPass.Parse(reader, context));
                        break;

                    case W3dChunkType.W3D_CHUNK_AABTREE:
                        aabTree = W3dMeshAabTree.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_MESH_USER_TEXT:
                        userText = W3dMeshUserText.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_VERTICES_2:
                        vertices2 = W3dVector3List.Parse(reader, context, chunkType);
                        break;

                    case W3dChunkType.W3D_CHUNK_NORMALS_2:
                        normals2 = W3dVector3List.Parse(reader, context, chunkType);
                        break;

                    case W3dChunkType.W3D_CHUNK_TANGENTS:
                        tangents = W3dVector3List.Parse(reader, context, chunkType);
                        break;

                    case W3dChunkType.W3D_CHUNK_BITANGENTS:
                        bitangents = W3dVector3List.Parse(reader, context, chunkType);
                        break;

                    case W3dChunkType.W3D_CHUNK_SHADER_MATERIALS:
                        shaderMaterials = W3dShaderMaterials.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_PRELIT_VERTEX:
                        prelitVertex = W3dPrelitVertex.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_PRELIT_LIGHTMAP_MULTI_PASS:
                        prelitLightMapMultiPass = W3dPrelitLightMapMultiPass.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_PRELIT_LIGHTMAP_MULTI_TEXTURE:
                        prelitLightMapMultiTexture = W3dPrelitLightMapMultiTexture.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_PRELIT_UNLIT:
                        prelitUnlit = W3dPrelitUnlit.Parse(reader, context);
                        break;

                    default:
                        throw CreateUnknownChunkException(chunkType);
                }
            });

            if (resultHeader is null || vertices is null || shadeIndices is null || materialInfo is null || normals is null || triangles is null)
            {
                throw new InvalidDataException();
            }

            return new W3dMesh(resultHeader, userText, vertices, normals, vertices2, normals2, tangents, bitangents,
                influences, triangles, shadeIndices, materialInfo, vertexMaterials, shaders, shadersPs2, textures,
                materialPasses, aabTree, shaderMaterials, prelitVertex, prelitLightMapMultiPass,
                prelitLightMapMultiTexture, prelitUnlit);
        });
    }

    protected override IEnumerable<W3dChunk> GetSubChunksOverride()
    {
        yield return Header;

        if (UserText != null)
        {
            yield return UserText;
        }

        yield return Vertices;

        if (Vertices2 != null)
        {
            yield return Vertices2;
        }

        yield return Normals;

        if (Normals2 != null)
        {
            yield return Normals2;
        }

        if (Tangents != null)
        {
            yield return Tangents;
        }

        if (Bitangents != null)
        {
            yield return Bitangents;
        }

        yield return Triangles;

        if (Influences != null)
        {
            yield return Influences;
        }

        yield return ShadeIndices;
        yield return MaterialInfo;

        if (ShaderMaterials != null)
        {
            yield return ShaderMaterials;
        }

        if (VertexMaterials != null)
        {
            yield return VertexMaterials;
        }

        if (Shaders != null)
        {
            yield return Shaders;
        }

        if (ShadersPs2 != null)
        {
            yield return ShadersPs2;
        }

        if (Textures != null)
        {
            yield return Textures;
        }

        foreach (var materialPass in MaterialPasses)
        {
            yield return materialPass;
        }

        if (AabTree != null)
        {
            yield return AabTree;
        }
    }
}

/// <summary>
/// This has line-separated key/value pairs
/// for things like mass, elasticity, friction, etc.
/// </summary>
public sealed record W3dMeshUserText(string Value) : W3dChunk(W3dChunkType.W3D_CHUNK_MESH_USER_TEXT)
{
    internal static W3dMeshUserText Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var value = reader.ReadFixedLengthString((int)header.ChunkSize);

            return new W3dMeshUserText(value);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        writer.WriteFixedLengthString(Value, Value.Length + 1);
    }
}

public sealed record W3dVector3List(W3dChunkType ChunkType, Vector3[] Items)
    : W3dStructListChunk<Vector3>(ChunkType, Items)
{
    internal static W3dVector3List Parse(BinaryReader reader, W3dParseContext context, W3dChunkType chunkType)
    {
        return ParseChunk(reader, context, header =>
        {
            var items = ParseItems(header, reader, r => r.ReadVector3());

            return new W3dVector3List(chunkType, items);
        });
    }

    protected override void WriteItem(BinaryWriter writer, in Vector3 item)
    {
        writer.Write(item);
    }
}

public sealed record W3dVector2List(W3dChunkType ChunkType, Vector2[] Items)
    : W3dStructListChunk<Vector2>(ChunkType, Items)
{
    internal static W3dVector2List Parse(BinaryReader reader, W3dParseContext context, W3dChunkType chunkType)
    {
        return ParseChunk(reader, context, header =>
        {
            var items = ParseItems(header, reader, r => r.ReadVector2());

            return new W3dVector2List(chunkType, items);
        });
    }

    protected override void WriteItem(BinaryWriter writer, in Vector2 item)
    {
        writer.Write(item);
    }
}

public sealed record W3dUInt32List(W3dChunkType ChunkType, uint[] Items) : W3dStructListChunk<uint>(ChunkType, Items)
{
    internal static W3dUInt32List Parse(BinaryReader reader, W3dParseContext context, W3dChunkType chunkType)
    {
        return ParseChunk(reader, context, header =>
        {
            var items = ParseItems(header, reader, r => r.ReadUInt32());

            return new W3dUInt32List(chunkType, items);
        });
    }

    protected override void WriteItem(BinaryWriter writer, in uint item)
    {
        writer.Write(item);
    }
}
