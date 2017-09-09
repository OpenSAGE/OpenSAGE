using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using LLGfx;
using OpenSage.Data;
using OpenSage.Data.W3d;
using OpenSage.Graphics.Util;
using OpenSage.Mathematics;

namespace OpenSage.Graphics
{
    public sealed class ModelMesh : GraphicsObject
    {
        private readonly uint _numVertices;
        private readonly Buffer _vertexBuffer;

        private readonly uint _numIndices;
        private readonly Buffer _indexBuffer;

        private readonly DescriptorSet _pixelMeshDescriptorSet;

        private readonly DynamicBuffer _meshTransformConstantBuffer;
        private MeshTransformConstants _meshTransformConstants;

        private readonly DynamicBuffer _perDrawConstantBuffer;
        private PerDrawConstants _perDrawConstants;

        public string Name { get; }

        public ModelBone ParentBone { get; }

        public BoundingSphere BoundingSphere { get; }

        public IReadOnlyList<ModelMeshMaterialPass> MaterialPasses { get; }

        public bool Skinned { get; }

        internal ModelMesh(
            GraphicsDevice graphicsDevice,
            ResourceUploadBatch uploadBatch,
            W3dMesh w3dMesh,
            ModelBone parentBone,
            FileSystem fileSystem,
            DescriptorSetLayout pixelMeshDescriptorSetLayout,
            DescriptorSetLayout vertexMaterialPassDescriptorSetLayout,
            DescriptorSetLayout pixelMaterialPassDescriptorSetLayout,
            ModelRenderer modelRenderer)
        {
            Name = w3dMesh.Header.MeshName;
            ParentBone = parentBone;

            BoundingSphere = new BoundingSphere(
                w3dMesh.Header.SphCenter.ToVector3(),
                w3dMesh.Header.SphRadius);

            Skinned = w3dMesh.Header.Attributes.HasFlag(W3dMeshFlags.GeometryTypeSkin);

            _numVertices = (uint) w3dMesh.Vertices.Length;

            _vertexBuffer = CreateVertexBuffer(
                graphicsDevice,
                uploadBatch,
                w3dMesh,
                Skinned);

            _numIndices = (uint) w3dMesh.Triangles.Length * 3;

            _indexBuffer = CreateIndexBuffer(
                graphicsDevice,
                uploadBatch,
                w3dMesh);

            var materialsBuffer = CreateMaterialsBuffer(
                graphicsDevice,
                uploadBatch,
                w3dMesh);

            var textures = CreateTextures(
                graphicsDevice,
                uploadBatch,
                w3dMesh,
                fileSystem);

            _pixelMeshDescriptorSet = AddDisposable(new DescriptorSet(
                graphicsDevice,
                pixelMeshDescriptorSetLayout));

            _pixelMeshDescriptorSet.SetStructuredBuffer(0, materialsBuffer);

            _pixelMeshDescriptorSet.SetTextures(1, textures);

            var remainingTextures = ModelRenderer.MaxTextures - textures.Length;
            _pixelMeshDescriptorSet.SetTextures(1 + textures.Length, new Texture[remainingTextures]);

            _meshTransformConstantBuffer = AddDisposable(DynamicBuffer.Create<MeshTransformConstants>(graphicsDevice));

            _perDrawConstantBuffer = AddDisposable(DynamicBuffer.Create<PerDrawConstants>(graphicsDevice));

            var materialPasses = new List<ModelMeshMaterialPass>();
            foreach (var w3dMaterialPass in w3dMesh.MaterialPasses)
            {
                materialPasses.Add(AddDisposable(new ModelMeshMaterialPass(
                    graphicsDevice,
                    uploadBatch,
                    w3dMesh,
                    w3dMaterialPass,
                    vertexMaterialPassDescriptorSetLayout,
                    pixelMaterialPassDescriptorSetLayout,
                    modelRenderer)));
            }
            MaterialPasses = materialPasses;
        }

        public void SetMatrices(
            ref Matrix4x4 world, 
            ref Matrix4x4 view, 
            ref Matrix4x4 projection)
        {
            _meshTransformConstants.WorldViewProjection = world * view * projection;
            _meshTransformConstants.World = world;
            _meshTransformConstants.SkinningEnabled = Skinned;

            _meshTransformConstantBuffer.SetData(ref _meshTransformConstants);
        }

        private StaticBuffer CreateMaterialsBuffer(
            GraphicsDevice graphicsDevice,
            ResourceUploadBatch uploadBatch,
            W3dMesh w3dMesh)
        {
            var vertexMaterials = new VertexMaterial[w3dMesh.Materials.Length];

            for (var i = 0; i < w3dMesh.Materials.Length; i++)
            {
                var w3dVertexMaterial = w3dMesh.Materials[i].VertexMaterialInfo;

                var textureMapping = TextureMappingType.Uv;
                if (w3dVertexMaterial.Attributes.HasFlag(W3dVertexMaterialFlags.Stage0MappingEnvironment))
                    textureMapping = TextureMappingType.Environment;
                else if (w3dVertexMaterial.Attributes.HasFlag(W3dVertexMaterialFlags.Stage0MappingLinearOffset))
                    textureMapping = TextureMappingType.LinearOffset;

                var mapperUVPerSec = Vector2.Zero;
                var mapperArgs0 = w3dMesh.Materials[i].MapperArgs0;
                if (!string.IsNullOrEmpty(mapperArgs0))
                {
                    var splitMapperArgs0 = mapperArgs0.Split(new[] { "\r\n" }, System.StringSplitOptions.RemoveEmptyEntries);
                    foreach (var mapperArg in splitMapperArgs0)
                    {
                        var splitMapperArg = mapperArg.Split('=');
                        switch (splitMapperArg[0])
                        {
                            case "UPerSec":
                                mapperUVPerSec.X = float.Parse(splitMapperArg[1]);
                                break;

                            case "VPerSec":
                                mapperUVPerSec.Y = float.Parse(splitMapperArg[1]);
                                break;

                            default:
                                throw new System.NotImplementedException();
                        }
                    }
                }

                vertexMaterials[i] = new VertexMaterial
                {
                    Ambient = w3dVertexMaterial.Ambient.ToVector3(),
                    Diffuse = w3dVertexMaterial.Diffuse.ToVector3(),
                    Specular = w3dVertexMaterial.Specular.ToVector3(),
                    Emissive = w3dVertexMaterial.Emissive.ToVector3(),
                    Shininess = w3dVertexMaterial.Shininess,
                    Opacity = w3dVertexMaterial.Opacity,
                    TextureMapping = textureMapping,
                    TextureMapperUVPerSec = mapperUVPerSec
                };
            }

            return AddDisposable(StaticBuffer.Create(
                graphicsDevice,
                uploadBatch,
                vertexMaterials,
                false));
        }

        private Texture[] CreateTextures(
            GraphicsDevice graphicsDevice,
            ResourceUploadBatch uploadBatch,
            W3dMesh w3dMesh,
            FileSystem fileSystem)
        {
            var numTextures = w3dMesh.Textures.Length;
            var textures = new Texture[numTextures];
            for (var i = 0; i < numTextures; i++)
            {
                var w3dTexture = w3dMesh.Textures[i];

                var fileExtensions = new[] { ".dds", ".tga" };
                foreach (var fileExtension in fileExtensions)
                {
                    var textureName = w3dTexture.Name.Replace(".tga", fileExtension);
                    var texturePath = $@"Art\Textures\{textureName}";

                    var textureFileSystemEntry = fileSystem.GetFile(texturePath);

                    if (textureFileSystemEntry == null)
                    {
                        continue;
                    }

                    textures[i] = AddDisposable(TextureLoader.LoadTexture(
                        graphicsDevice,
                        uploadBatch,
                        textureFileSystemEntry));
                }

                if (textures[i] == null)
                {
                    throw new System.InvalidOperationException();
                }
            }
            return textures;
        }

        private StaticBuffer CreateVertexBuffer(
            GraphicsDevice graphicsDevice,
            ResourceUploadBatch uploadBatch,
            W3dMesh w3dMesh,
            bool isSkinned)
        {
            var vertices = new MeshVertex[_numVertices];

            for (var i = 0; i < _numVertices; i++)
            {
                vertices[i] = new MeshVertex
                {
                    Position = w3dMesh.Vertices[i].ToVector3(),
                    Normal = w3dMesh.Normals[i].ToVector3(),
                    BoneIndex = isSkinned
                        ? w3dMesh.Influences[i].BoneIndex
                        : 0u
                };
            }

            return AddDisposable(StaticBuffer.Create(
                graphicsDevice,
                uploadBatch,
                vertices,
                false));
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MeshVertex
        {
            public const int SizeInBytes = sizeof(float) * 6 + sizeof(uint);

            public Vector3 Position;
            public Vector3 Normal;
            public uint BoneIndex;
        }

        private StaticBuffer CreateIndexBuffer(
            GraphicsDevice graphicsDevice,
            ResourceUploadBatch uploadBatch,
            W3dMesh w3dMesh)
        {
            var indices = new ushort[_numIndices];

            var indexIndex = 0;
            foreach (var triangle in w3dMesh.Triangles)
            {
                indices[indexIndex++] = (ushort) triangle.VIndex0;
                indices[indexIndex++] = (ushort) triangle.VIndex1;
                indices[indexIndex++] = (ushort) triangle.VIndex2;
            }

            return AddDisposable(StaticBuffer.Create(
                graphicsDevice,
                uploadBatch,
                indices,
                false));
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MeshTransformConstants
        {
            public Matrix4x4 WorldViewProjection;
            public Matrix4x4 World;
            public bool SkinningEnabled;
        }

        public void Draw(CommandEncoder commandEncoder, bool alphaBlended)
        {
            void drawImpl(PipelineState pipelineState)
            {
                commandEncoder.SetPipelineState(pipelineState);

                commandEncoder.SetInlineConstantBuffer(1, _meshTransformConstantBuffer);

                commandEncoder.SetDescriptorSet(6, _pixelMeshDescriptorSet);

                commandEncoder.SetVertexBuffer(0, _vertexBuffer);

                var materialPassIndex = 0u;
                foreach (var materialPass in MaterialPasses)
                {
                    if (!materialPass.MeshParts.Any(x => x.PipelineState == pipelineState))
                    {
                        materialPassIndex++;
                        continue;
                    }

                    commandEncoder.SetDescriptorSet(4, materialPass.VertexMaterialPassDescriptorSet);
                    commandEncoder.SetDescriptorSet(5, materialPass.PixelMaterialPassDescriptorSet);

                    commandEncoder.SetVertexBuffer(1, materialPass.TexCoordVertexBuffer);

                    foreach (var meshPart in materialPass.MeshParts)
                    {
                        if (meshPart.PipelineState != pipelineState)
                        {
                            continue;
                        }

                        _perDrawConstants.PrimitiveOffset = meshPart.StartIndex / 3;
                        _perDrawConstants.NumTextureStages = materialPass.NumTextureStages;
                        _perDrawConstants.AlphaTest = meshPart.AlphaTest;
                        _perDrawConstants.Texturing = meshPart.Texturing;

                        // TODO: Use time from main game engine, don't query for it every time like this.
                        _perDrawConstants.TimeSecondsFraction = (float) System.DateTime.Now.TimeOfDay.TotalSeconds;

                        _perDrawConstantBuffer.SetData(ref _perDrawConstants);
                        commandEncoder.SetInlineConstantBuffer(0, _perDrawConstantBuffer);

                        commandEncoder.DrawIndexed(
                            PrimitiveType.TriangleList,
                            meshPart.IndexCount,
                            IndexType.UInt16,
                            _indexBuffer,
                            meshPart.StartIndex);
                    }

                    materialPassIndex++;
                }
            }

            // TODO: Don't do this here.
            var uniquePipelineStates = MaterialPasses
                .SelectMany(x => x.MeshParts.Select(y => y.PipelineState))
                .Distinct();

            foreach (var pipelineState in uniquePipelineStates)
            {
                if (pipelineState.Description.Blending.Enabled == alphaBlended)
                {
                    drawImpl(pipelineState);
                }
            }
        }

        [StructLayout(LayoutKind.Explicit, Size = 68)]
        private struct VertexMaterial
        {
            [FieldOffset(0)]
            public Vector3 Ambient;

            [FieldOffset(12)]
            public Vector3 Diffuse;

            [FieldOffset(24)]
            public Vector3 Specular;

            [FieldOffset(36)]
            public float Shininess;

            [FieldOffset(40)]
            public Vector3 Emissive;

            [FieldOffset(52)]
            public float Opacity;

            [FieldOffset(56)]
            public TextureMappingType TextureMapping;

            [FieldOffset(60)]
            public Vector2 TextureMapperUVPerSec;
        }

        public enum TextureMappingType : uint
        {
            Uv           = 0,
            Environment  = 1,
            LinearOffset = 2
        }

        [StructLayout(LayoutKind.Explicit, Size = 20)]
        private struct PerDrawConstants
        {
            [FieldOffset(0)]
            public uint PrimitiveOffset;

            // Not actually per-draw, but we don't have a per-mesh CB.
            [FieldOffset(4)]
            public uint NumTextureStages;

            [FieldOffset(8)]
            public bool AlphaTest;

            [FieldOffset(12)]
            public bool Texturing;

            [FieldOffset(16)]
            public float TimeSecondsFraction;
        }
    }
}
