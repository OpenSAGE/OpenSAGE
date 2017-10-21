using System.Collections.Generic;
using System.IO;
using System.Numerics;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dMesh : W3dChunk
    {
        public W3dMeshHeader3 Header { get; private set; }

        public Vector3[] Vertices { get; private set; }
        public Vector3[] Normals { get; private set; }

        // TODO: What are these? Some sort of alternate state?
        public Vector3[] Vertices2 { get; private set; }
        public Vector3[] Normals2 { get; private set; }

        // TODO: Implement these in renderer.
        public Vector3[] Tangents { get; private set; }
        public Vector3[] Bitangents { get; private set; }

        /// <summary>
        /// Vertex influences link vertices of a mesh to bones in a hierarchy.
        /// This is the information needed for skinning.
        /// </summary>
        public W3dVertexInfluence[] Influences { get; private set; }

        public W3dTriangle[] Triangles { get; private set; }

        /// <summary>
        /// shade indexes for each vertex (array of uint32's)
        /// </summary>
        public uint[] ShadeIndices { get; private set; }

        public W3dMaterialInfo MaterialInfo { get; private set; }

        public W3dMaterial[] Materials { get; private set; }

        public W3dShader[] Shaders { get; private set; }

        public IReadOnlyList<W3dTexture> Textures { get; private set; }

        public W3dMaterialPass[] MaterialPasses { get; private set; }

        public W3dMeshAabTree AabTree { get; private set; }

        public W3dShaderMaterials ShaderMaterials { get; private set; }

        public static W3dMesh Parse(BinaryReader reader, uint chunkSize)
        {
            var currentMaterialPass = 0;

            var textures = new List<W3dTexture>();

            var finalResult = ParseChunk<W3dMesh>(reader, chunkSize, (result, header) =>
            {
                switch (header.ChunkType)
                {
                    case W3dChunkType.W3D_CHUNK_MESH_HEADER3:
                        result.Header = W3dMeshHeader3.Parse(reader);
                        result.Vertices = new Vector3[result.Header.NumVertices];
                        result.Normals = new Vector3[result.Header.NumVertices];
                        result.ShadeIndices = new uint[result.Header.NumVertices];
                        result.Influences = new W3dVertexInfluence[result.Header.NumVertices];
                        result.Triangles = new W3dTriangle[result.Header.NumTris];
                        break;

                    case W3dChunkType.W3D_CHUNK_VERTICES:
                        for (var count = 0; count < result.Header.NumVertices; count++)
                        {
                            result.Vertices[count] = reader.ReadVector3();
                        }
                        break;

                    case W3dChunkType.W3D_CHUNK_VERTEX_NORMALS:
                        for (var count = 0; count < result.Header.NumVertices; count++)
                        {
                            result.Normals[count] = reader.ReadVector3();
                        }
                        break;

                    case W3dChunkType.W3D_CHUNK_VERTEX_INFLUENCES:
                        for (var count = 0; count < result.Header.NumVertices; count++)
                        {
                            result.Influences[count] = W3dVertexInfluence.Parse(reader);
                        }
                        break;

                    case W3dChunkType.W3D_CHUNK_TRIANGLES:
                        for (var count = 0; count < result.Header.NumTris; count++)
                        {
                            result.Triangles[count] = W3dTriangle.Parse(reader);
                        }
                        break;

                    case W3dChunkType.W3D_CHUNK_VERTEX_SHADE_INDICES:
                        for (var count = 0; count < result.Header.NumVertices; count++)
                        {
                            result.ShadeIndices[count] = reader.ReadUInt32();
                        }
                        break;

                    case W3dChunkType.W3D_CHUNK_MATERIAL_INFO:
                        result.MaterialInfo = W3dMaterialInfo.Parse(reader);
                        result.Materials = new W3dMaterial[result.MaterialInfo.VertexMaterialCount];
                        result.Shaders = new W3dShader[result.MaterialInfo.ShaderCount];
                        result.MaterialPasses = new W3dMaterialPass[result.MaterialInfo.PassCount];
                        break;

                    case W3dChunkType.W3D_CHUNK_VERTEX_MATERIALS:
                        for (var count = 0; count < result.MaterialInfo.VertexMaterialCount; count++)
                        {
                            var innerChunk = W3dChunkHeader.Parse(reader);
                            if (innerChunk.ChunkType == W3dChunkType.W3D_CHUNK_VERTEX_MATERIAL)
                            {
                                result.Materials[count] = W3dMaterial.Parse(reader, innerChunk.ChunkSize);
                            }
                            else
                            {
                                throw CreateUnknownChunkException(innerChunk);
                            }
                        }
                        break;

                    case W3dChunkType.W3D_CHUNK_SHADERS:
                        for (var count = 0; count < result.MaterialInfo.ShaderCount; count++)
                        {
                            result.Shaders[count] = W3dShader.Parse(reader);
                        }
                        break;

                    case W3dChunkType.W3D_CHUNK_TEXTURES:
                        var startPosition = reader.BaseStream.Position;
                        while (reader.BaseStream.Position < startPosition + header.ChunkSize)
                        {
                            var innerChunk = W3dChunkHeader.Parse(reader);
                            if (innerChunk.ChunkType == W3dChunkType.W3D_CHUNK_TEXTURE)
                            {
                                textures.Add(W3dTexture.Parse(reader, innerChunk.ChunkSize));
                            }
                            else
                            {
                                throw CreateUnknownChunkException(innerChunk);
                            }
                        }
                        break;

                    case W3dChunkType.W3D_CHUNK_MATERIAL_PASS:
                        result.MaterialPasses[currentMaterialPass] = W3dMaterialPass.Parse(reader, header.ChunkSize);
                        currentMaterialPass++;
                        break;

                    case W3dChunkType.W3D_CHUNK_AABTREE:
                        if (result.AabTree != null)
                        {
                            throw new InvalidDataException();
                        }
                        result.AabTree = W3dMeshAabTree.Parse(reader, header.ChunkSize);
                        break;

                    case W3dChunkType.W3D_CHUNK_PS2_SHADERS:
                        // Don't need this.
                        reader.ReadBytes((int) header.ChunkSize);
                        break;

                    case W3dChunkType.W3D_CHUNK_MESH_USER_TEXT:
                        // TODO: Do we need this? It has line-separated key/value pairs
                        // for things like mass, elasticity, friction, etc.
                        reader.ReadBytes((int) header.ChunkSize);
                        break;

                    case W3dChunkType.W3D_CHUNK_VERTICES_2:
                        result.Vertices2 = new Vector3[result.Header.NumVertices];
                        for (var count = 0; count < result.Vertices2.Length; count++)
                        {
                            result.Vertices2[count] = reader.ReadVector3();
                        }
                        break;

                    case W3dChunkType.W3D_CHUNK_NORMALS_2:
                        result.Normals2 = new Vector3[result.Header.NumVertices];
                        for (var count = 0; count < result.Normals2.Length; count++)
                        {
                            result.Normals2[count] = reader.ReadVector3();
                        }
                        break;

                    case W3dChunkType.W3D_CHUNK_TANGENTS:
                        result.Tangents = new Vector3[result.Header.NumVertices];
                        for (var count = 0; count < result.Tangents.Length; count++)
                        {
                            result.Tangents[count] = reader.ReadVector3();
                        }
                        break;

                    case W3dChunkType.W3D_CHUNK_BITANGENTS:
                        result.Bitangents = new Vector3[result.Header.NumVertices];
                        for (var count = 0; count < result.Bitangents.Length; count++)
                        {
                            result.Bitangents[count] = reader.ReadVector3();
                        }
                        break;

                    case W3dChunkType.W3D_CHUNK_SHADER_MATERIALS:
                        result.ShaderMaterials = W3dShaderMaterials.Parse(reader, header.ChunkSize);
                        break;

                    default:
                        throw CreateUnknownChunkException(header);
                }
            });

            finalResult.Textures = textures;

            return finalResult;
        }
    }
}
