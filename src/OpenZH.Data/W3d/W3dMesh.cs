using System.IO;

namespace OpenZH.Data.W3d
{
    public sealed class W3dMesh
    {
        public W3dMeshHeader3 Header { get; private set; }
        public W3dVector[] Vertices { get; private set; }
        public W3dVector[] Normals { get; private set; }

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

        public W3dTexture[] Textures { get; private set; }

        public W3dMaterialPass[] MaterialPasses { get; private set; }

        public static W3dMesh Parse(BinaryReader reader, uint chunkSize)
        {
            var result = new W3dMesh();

            uint loadedSize = 0;
            var currentMaterialPass = 0;

            do
            {
                loadedSize += W3dChunkHeader.SizeInBytes;
                var currentChunk = W3dChunkHeader.Parse(reader);

                loadedSize += currentChunk.ChunkSize;

                switch (currentChunk.ChunkType)
                {
                    case W3dChunkType.W3D_CHUNK_MESH_HEADER3:
                        result.Header = W3dMeshHeader3.Parse(reader);
                        result.Vertices = new W3dVector[result.Header.NumVertices];
                        result.Normals = new W3dVector[result.Header.NumVertices];
                        result.ShadeIndices = new uint[result.Header.NumVertices];
                        result.Influences = new W3dVertexInfluence[result.Header.NumVertices];
                        result.Triangles = new W3dTriangle[result.Header.NumTris];
                        break;

                    case W3dChunkType.W3D_CHUNK_VERTICES:
                        for (var count = 0; count < result.Header.NumVertices; count++)
                        {
                            result.Vertices[count] = W3dVector.Parse(reader);
                        }
                        break;

                    case W3dChunkType.W3D_CHUNK_VERTEX_NORMALS:
                        for (var count = 0; count < result.Header.NumVertices; count++)
                        {
                            result.Normals[count] = W3dVector.Parse(reader);
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
                        result.Textures = new W3dTexture[result.MaterialInfo.TextureCount];
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
                                reader.ReadBytes((int) innerChunk.ChunkSize);
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
                        for (var count = 0; count < result.MaterialInfo.TextureCount; count++)
                        {
                            var innerChunk = W3dChunkHeader.Parse(reader);
                            if (innerChunk.ChunkType == W3dChunkType.W3D_CHUNK_TEXTURE)
                            {
                                result.Textures[count] = W3dTexture.Parse(reader, innerChunk.ChunkSize);
                            }
                            else
                            {
                                reader.ReadBytes((int) innerChunk.ChunkSize);
                            }
                        }
                        break;

                    case W3dChunkType.W3D_CHUNK_MATERIAL_PASS:
                        result.MaterialPasses[currentMaterialPass] = W3dMaterialPass.Parse(reader, currentChunk.ChunkSize);
                        currentMaterialPass++;
                        break;

                    case W3dChunkType.W3D_CHUNK_PS2_SHADERS:
                    case W3dChunkType.W3D_CHUNK_AABTREE:
                        // Ignored for now.
                        reader.ReadBytes((int) currentChunk.ChunkSize);
                        break;

                    default:
                        throw new InvalidDataException($"Unknown chunk type: {currentChunk.ChunkType}");
                }

            } while (loadedSize < chunkSize);

            return result;
        }
    }
}
