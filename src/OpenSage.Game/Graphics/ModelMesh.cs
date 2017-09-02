using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.Data.W3d;
using OpenSage.Graphics.Util;
using LLGfx;
using System.Collections.Generic;
using OpenSage.Data;

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

        public string Name { get; }

        public IReadOnlyList<ModelMeshMaterialPass> MaterialPasses { get; }

        internal ModelMesh(
            GraphicsDevice graphicsDevice,
            ResourceUploadBatch uploadBatch,
            W3dMesh w3dMesh,
            FileSystem fileSystem,
            DescriptorSetLayout pixelMeshDescriptorSetLayout,
            DescriptorSetLayout vertexMaterialPassDescriptorSetLayout,
            DescriptorSetLayout pixelMaterialPassDescriptorSetLayout)
        {
            Name = w3dMesh.Header.MeshName;

            _numVertices = (uint) w3dMesh.Vertices.Length;

            _vertexBuffer = CreateVertexBuffer(
                graphicsDevice,
                uploadBatch,
                w3dMesh);

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

            _meshTransformConstantBuffer = DynamicBuffer.Create<MeshTransformConstants>(graphicsDevice);

            var materialPasses = new List<ModelMeshMaterialPass>();
            foreach (var w3dMaterialPass in w3dMesh.MaterialPasses)
            {
                // TODO: Don't do this.
                if (w3dMaterialPass.TextureStages.Length == 0)
                    continue;

                materialPasses.Add(AddDisposable(new ModelMeshMaterialPass(
                    graphicsDevice,
                    uploadBatch,
                    w3dMesh,
                    w3dMaterialPass,
                    vertexMaterialPassDescriptorSetLayout,
                    pixelMaterialPassDescriptorSetLayout)));
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
                vertexMaterials[i] = new VertexMaterial
                {
                    Ambient = w3dVertexMaterial.Ambient.ToVector3(),
                    Diffuse = w3dVertexMaterial.Diffuse.ToVector3(),
                    Specular = w3dVertexMaterial.Specular.ToVector3(),
                    Emissive = w3dVertexMaterial.Emissive.ToVector3(),
                    Shininess = w3dVertexMaterial.Shininess,
                    Opacity = w3dVertexMaterial.Opacity
                };
            }

            return AddDisposable(StaticBuffer.Create(
                graphicsDevice,
                uploadBatch,
                vertexMaterials));
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
                var textureName = w3dTexture.Name.Replace(".tga", ".dds"); // TODO: Is this always right?
                var texturePath = $@"Art\Textures\{textureName}";

                var textureFileSystemEntry = fileSystem.GetFile(texturePath);

                textures[i] = AddDisposable(TextureLoader.LoadTexture(
                    graphicsDevice,
                    uploadBatch,
                    textureFileSystemEntry));
            }
            return textures;
        }

        private StaticBuffer CreateVertexBuffer(
            GraphicsDevice graphicsDevice,
            ResourceUploadBatch uploadBatch,
            W3dMesh w3dMesh)
        {
            var vertices = new MeshVertex[_numVertices];

            for (var i = 0; i < _numVertices; i++)
            {
                vertices[i] = new MeshVertex
                {
                    Position = w3dMesh.Vertices[i].ToVector3(),
                    Normal = w3dMesh.Normals[i].ToVector3()
                };
            }

            return AddDisposable(StaticBuffer.Create(
                graphicsDevice,
                uploadBatch,
                vertices));
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MeshVertex
        {
            public const int SizeInBytes = sizeof(float) * 6;

            public Vector3 Position;
            public Vector3 Normal;
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
                indices));
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MeshTransformConstants
        {
            public const int SizeInBytes = 128;

            public Matrix4x4 WorldViewProjection;
            public Matrix4x4 World;
        }

        public void Draw(CommandEncoder commandEncoder)
        {
            commandEncoder.SetInlineConstantBuffer(0, _meshTransformConstantBuffer);

            commandEncoder.SetDescriptorSet(4, _pixelMeshDescriptorSet);

            commandEncoder.SetVertexBuffer(0, _vertexBuffer);

            foreach (var materialPass in MaterialPasses)
            {
                commandEncoder.SetDescriptorSet(2, materialPass.VertexMaterialPassDescriptorSet);
                commandEncoder.SetDescriptorSet(3, materialPass.PixelMaterialPassDescriptorSet);

                commandEncoder.SetVertexBuffer(1, materialPass.TexCoordVertexBuffer);

                foreach (var meshPart in materialPass.MeshParts)
                {
                    // TODO: Set alpha blending state etc.
                    // based on W3dShader.

                    commandEncoder.DrawIndexed(
                        PrimitiveType.TriangleList,
                        meshPart.IndexCount,
                        IndexType.UInt16,
                        _indexBuffer,
                        meshPart.StartIndex);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct VertexMaterial
        {
            public const int SizeInBytes = 14 * sizeof(float);

            public Vector3 Ambient;

            public Vector3 Diffuse;

            public Vector3 Specular;

            public float Shininess;

            public Vector3 Emissive;

            public float Opacity;
        }
    }
}
