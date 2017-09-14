using System.Numerics;
using OpenSage.Data.W3d;
using OpenSage.Graphics.Util;
using LLGfx;
using System.Collections.Generic;

namespace OpenSage.Graphics
{
    // One ModelMeshMaterialPass for each W3D_CHUNK_MATERIAL_PASS
    public sealed class ModelMeshMaterialPass : GraphicsObject
    {
        public DescriptorSet VertexMaterialPassDescriptorSet;
        public DescriptorSet PixelMaterialPassDescriptorSet;

        public IReadOnlyList<ModelMeshPart> MeshParts { get; }

        public uint NumTextureStages { get; }

        public StaticBuffer TexCoordVertexBuffer { get; }

        internal ModelMeshMaterialPass(
            GraphicsDevice graphicsDevice,
            ResourceUploadBatch uploadBatch,
            W3dMesh w3dMesh, 
            W3dMaterialPass w3dMaterialPass,
            DescriptorSetLayout vertexMaterialPassDescriptorSetLayout,
            DescriptorSetLayout pixelMaterialPassDescriptorSetLayout,
            ModelRenderer modelRenderer)
        {
            var hasTextureStage0 = w3dMaterialPass.TextureStages.Count > 0;
            var textureStage0 = hasTextureStage0
                ? w3dMaterialPass.TextureStages[0]
                : null;

            var hasTextureStage1 = w3dMaterialPass.TextureStages.Count > 1;
            var textureStage1 = hasTextureStage1
                ? w3dMaterialPass.TextureStages[1]
                : null;

            var texCoords = new TexCoords[w3dMesh.Header.NumVertices];

            if (hasTextureStage0)
            {
                for (var i = 0; i < texCoords.Length; i++)
                {
                    texCoords[i].UV0 = textureStage0.TexCoords[i].ToVector2();
                    if (hasTextureStage1)
                    {
                        texCoords[i].UV1 = textureStage1.TexCoords[i].ToVector2();
                    }
                }
            }

            TexCoordVertexBuffer = AddDisposable(StaticBuffer.Create(
                graphicsDevice,
                uploadBatch,
                texCoords,
                false));

            var textureIDs = new TextureIndex[w3dMesh.Header.NumTris];

            if (hasTextureStage0)
            {
                if (textureStage0.TextureIds.Length == 1)
                {
                    var textureID = textureStage0.TextureIds[0];
                    for (var i = 0; i < textureIDs.Length; i++)
                    {
                        textureIDs[i].IndexStage0 = textureID;
                    }
                }
                else
                {
                    for (var i = 0; i < textureIDs.Length; i++)
                    {
                        textureIDs[i].IndexStage0 = textureStage0.TextureIds[i];
                    }
                }
            }

            if (hasTextureStage1)
            {
                if (textureStage1.TextureIds.Length == 1)
                {
                    var textureID = textureStage1.TextureIds[0];
                    for (var i = 0; i < textureIDs.Length; i++)
                    {
                        textureIDs[i].IndexStage1 = textureID;
                    }
                }
                else
                {
                    for (var i = 0; i < textureIDs.Length; i++)
                    {
                        textureIDs[i].IndexStage1 = textureStage1.TextureIds[i];
                    }
                }
            }

            PixelMaterialPassDescriptorSet = AddDisposable(new DescriptorSet(
                graphicsDevice,
                pixelMaterialPassDescriptorSetLayout));

            var textureIndicesBuffer = AddDisposable(StaticBuffer.Create(
                graphicsDevice,
                uploadBatch,
                textureIDs,
                false));

            PixelMaterialPassDescriptorSet.SetTypedBuffer(0, textureIndicesBuffer, PixelFormat.R32UInt);

            var materialIDs = w3dMaterialPass.VertexMaterialIds;
            if (materialIDs.Length == 1)
            {
                var materialID = materialIDs[0];
                materialIDs = new uint[w3dMesh.Header.NumVertices];
                for (var i = 0; i < w3dMesh.Header.NumVertices; i++)
                {
                    materialIDs[i] = materialID;
                }
            }

            var materialIndicesBuffer = AddDisposable(StaticBuffer.Create(
                graphicsDevice,
                uploadBatch,
                materialIDs,
                false));

            VertexMaterialPassDescriptorSet = AddDisposable(new DescriptorSet(
                graphicsDevice,
                vertexMaterialPassDescriptorSetLayout));

            VertexMaterialPassDescriptorSet.SetTypedBuffer(0, materialIndicesBuffer, PixelFormat.R32UInt);

            var meshParts = new List<ModelMeshPart>();

            if (w3dMaterialPass.ShaderIds.Length == 1)
            {
                meshParts.Add(new ModelMeshPart(
                    0, 
                    w3dMesh.Header.NumTris * 3, 
                    w3dMesh,
                    w3dMesh.Shaders[w3dMaterialPass.ShaderIds[0]],
                    modelRenderer));
            }
            else
            {
                var shaderID = w3dMaterialPass.ShaderIds[0];
                var startIndex = 0u;
                var indexCount = 0u;
                for (var i = 0; i < w3dMaterialPass.ShaderIds.Length; i++)
                {
                    var newShaderID = w3dMaterialPass.ShaderIds[i];

                    if (shaderID != newShaderID)
                    {
                        meshParts.Add(new ModelMeshPart(
                            startIndex,
                            indexCount,
                            w3dMesh,
                            w3dMesh.Shaders[shaderID],
                            modelRenderer));

                        startIndex = (uint) (i * 3);
                        indexCount = 0;
                    }

                    shaderID = newShaderID;

                    indexCount += 3;
                }
            }

            MeshParts = meshParts;

            NumTextureStages = hasTextureStage0 && hasTextureStage1
                ? 2u
                : hasTextureStage0 ? 1u : 0u;
        }

        private struct TexCoords
        {
            public Vector2 UV0;
            public Vector2 UV1;
        }

        private struct TextureIndex
        {
            public uint IndexStage0;
            public uint IndexStage1;
        }
    }
}
