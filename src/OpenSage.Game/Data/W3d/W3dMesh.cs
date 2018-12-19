using System.Collections.Generic;
using System.IO;
using System.Numerics;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dMesh : W3dContainerChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_MESH;

        public W3dMeshHeader3 Header { get; private set; }

        public W3dMeshUserText UserText { get; private set; }

        public W3dVector3List Vertices { get; private set; }
        public W3dVector3List Normals { get; private set; }

        // TODO: What are these? Some sort of alternate state?
        public W3dVector3List Vertices2 { get; private set; }
        public W3dVector3List Normals2 { get; private set; }

        public W3dVector3List Tangents { get; private set; }
        public W3dVector3List Bitangents { get; private set; }

        public bool IsSkinned => (Header.Attributes & W3dMeshFlags.GeometryTypeMask) == W3dMeshFlags.GeometryTypeSkin;

        /// <summary>
        /// Vertex influences link vertices of a mesh to bones in a hierarchy.
        /// This is the information needed for skinning.
        /// </summary>
        public W3dVertexInfluences Influences { get; private set; }

        public W3dTriangles Triangles { get; private set; }

        /// <summary>
        /// shade indexes for each vertex (array of uint32's)
        /// </summary>
        public W3dUInt32List ShadeIndices { get; private set; }

        public W3dMaterialInfo MaterialInfo { get; private set; }

        public W3dVertexMaterials VertexMaterials { get; private set; }

        public W3dShaders Shaders { get; private set; }

        public W3dShadersPs2 ShadersPs2 { get; private set; }

        public W3dTextures Textures { get; private set; }

        public List<W3dMaterialPass> MaterialPasses { get; } = new List<W3dMaterialPass>();

        public W3dMeshAabTree AabTree { get; private set; }

        public W3dShaderMaterials ShaderMaterials { get; private set; }

        internal static W3dMesh Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new W3dMesh();

                ParseChunks(reader, context.CurrentEndPosition, chunkType =>
                {
                    switch (chunkType)
                    {
                        case W3dChunkType.W3D_CHUNK_MESH_HEADER3:
                            result.Header = W3dMeshHeader3.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_VERTICES:
                            result.Vertices = W3dVector3List.Parse(reader, context, chunkType);
                            break;

                        case W3dChunkType.W3D_CHUNK_VERTEX_NORMALS:
                            result.Normals = W3dVector3List.Parse(reader, context, chunkType);
                            break;

                        case W3dChunkType.W3D_CHUNK_VERTEX_INFLUENCES:
                            result.Influences = W3dVertexInfluences.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_TRIANGLES:
                            result.Triangles = W3dTriangles.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_VERTEX_SHADE_INDICES:
                            result.ShadeIndices = W3dUInt32List.Parse(reader, context, chunkType);
                            break;

                        case W3dChunkType.W3D_CHUNK_MATERIAL_INFO:
                            result.MaterialInfo = W3dMaterialInfo.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_VERTEX_MATERIALS:
                            result.VertexMaterials = W3dVertexMaterials.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_SHADERS:
                            result.Shaders = W3dShaders.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_PS2_SHADERS:
                            result.ShadersPs2 = W3dShadersPs2.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_TEXTURES:
                            result.Textures = W3dTextures.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_MATERIAL_PASS:
                            result.MaterialPasses.Add(W3dMaterialPass.Parse(reader, context));
                            break;

                        case W3dChunkType.W3D_CHUNK_AABTREE:
                            result.AabTree = W3dMeshAabTree.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_MESH_USER_TEXT:
                            result.UserText = W3dMeshUserText.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_VERTICES_2:
                            result.Vertices2 = W3dVector3List.Parse(reader, context, chunkType);
                            break;

                        case W3dChunkType.W3D_CHUNK_NORMALS_2:
                            result.Normals2 = W3dVector3List.Parse(reader, context, chunkType);
                            break;

                        case W3dChunkType.W3D_CHUNK_TANGENTS:
                            result.Tangents = W3dVector3List.Parse(reader, context, chunkType);
                            break;

                        case W3dChunkType.W3D_CHUNK_BITANGENTS:
                            result.Bitangents = W3dVector3List.Parse(reader, context, chunkType);
                            break;

                        case W3dChunkType.W3D_CHUNK_SHADER_MATERIALS:
                            result.ShaderMaterials = W3dShaderMaterials.Parse(reader, context);
                            break;

                        default:
                            throw CreateUnknownChunkException(chunkType);
                    }
                });

                return result;
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
    public sealed class W3dMeshUserText : W3dChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_MESH_USER_TEXT;

        public string Value { get; private set; }

        internal static W3dMeshUserText Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                return new W3dMeshUserText
                {
                    Value = reader.ReadFixedLengthString((int) header.ChunkSize)
                };
            });
        }

        protected override void WriteToOverride(BinaryWriter writer)
        {
            writer.WriteFixedLengthString(Value, Value.Length + 1);
        }
    }

    public sealed class W3dVector3List : W3dStructListChunk<W3dVector3List, Vector3>
    {
        private W3dChunkType _chunkType;

        public override W3dChunkType ChunkType => _chunkType;

        internal static W3dVector3List Parse(BinaryReader reader, W3dParseContext context, W3dChunkType chunkType)
        {
            var result = ParseList(reader, context, r => r.ReadVector3());
            result._chunkType = chunkType;
            return result;
        }

        protected override void WriteItem(BinaryWriter writer, in Vector3 item)
        {
            writer.Write(item);
        }
    }

    public sealed class W3dVector2List : W3dStructListChunk<W3dVector2List, Vector2>
    {
        private W3dChunkType _chunkType;

        public override W3dChunkType ChunkType => _chunkType;

        internal static W3dVector2List Parse(BinaryReader reader, W3dParseContext context, W3dChunkType chunkType)
        {
            var result = ParseList(reader, context, r => r.ReadVector2());
            result._chunkType = chunkType;
            return result;
        }

        protected override void WriteItem(BinaryWriter writer, in Vector2 item)
        {
            writer.Write(item);
        }
    }

    public sealed class W3dUInt32List : W3dStructListChunk<W3dUInt32List, uint>
    {
        private W3dChunkType _chunkType;

        public override W3dChunkType ChunkType => _chunkType;

        internal static W3dUInt32List Parse(BinaryReader reader, W3dParseContext context, W3dChunkType chunkType)
        {
            var result = ParseList(reader, context, r => r.ReadUInt32());
            result._chunkType = chunkType;
            return result;
        }

        protected override void WriteItem(BinaryWriter writer, in uint item)
        {
            writer.Write(item);
        }
    }
}
