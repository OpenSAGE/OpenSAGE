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

        public StaticBuffer TexCoordVertexBuffer { get; }

        internal ModelMeshMaterialPass(
            GraphicsDevice graphicsDevice,
            ResourceUploadBatch uploadBatch,
            W3dMesh w3dMesh, 
            W3dMaterialPass w3dMaterialPass,
            DescriptorSetLayout vertexMaterialPassDescriptorSetLayout,
            DescriptorSetLayout pixelMaterialPassDescriptorSetLayout)
        {
            // TODO: Support multiple textures stages.
            var textureStage = w3dMaterialPass.TextureStages[0];

            var texCoords = new Vector2[w3dMesh.Header.NumVertices];
            for (var i = 0; i < texCoords.Length; i++)
            {
                texCoords[i] = textureStage.TexCoords[i].ToVector2();
            }

            TexCoordVertexBuffer = AddDisposable(StaticBuffer.Create(
                graphicsDevice,
                uploadBatch,
                texCoords,
                false));

            var textureIDs = textureStage.TextureIds;
            if (textureIDs.Length == 1)
            {
                var textureID = textureIDs[0];
                textureIDs = new uint[w3dMesh.Header.NumTris];
                for (var i = 0; i < w3dMesh.Header.NumTris; i++)
                {
                    textureIDs[i] = textureID;
                }
            }

            var textureIndicesBuffer = AddDisposable(StaticBuffer.Create(
                graphicsDevice,
                uploadBatch,
                textureIDs,
                false));

            PixelMaterialPassDescriptorSet = AddDisposable(new DescriptorSet(
                graphicsDevice,
                pixelMaterialPassDescriptorSetLayout));

            PixelMaterialPassDescriptorSet.SetTypedBuffer(0, textureIndicesBuffer, PixelFormat.UInt32);

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

            VertexMaterialPassDescriptorSet.SetTypedBuffer(0, materialIndicesBuffer, PixelFormat.UInt32);

            var meshParts = new List<ModelMeshPart>();

            if (w3dMaterialPass.ShaderIds.Length == 1)
            {
                meshParts.Add(new ModelMeshPart(0, w3dMesh.Header.NumTris * 3));
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
                            indexCount));

                        startIndex = (uint) (i * 3);
                        indexCount = 0;
                    }

                    shaderID = newShaderID;

                    indexCount += 3;
                }
            }

            MeshParts = meshParts;
        }
    }
}
