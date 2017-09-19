using System.Collections.Generic;
using System.Numerics;
using LLGfx;
using OpenSage.Content;
using OpenSage.Data.W3d;
using OpenSage.Graphics.Util;

namespace OpenSage.Graphics
{
    // One ModelMeshMaterialPass for each W3D_CHUNK_MATERIAL_PASS
    public sealed class ModelMeshMaterialPass : GraphicsObject
    {
        internal ShaderResourceView TextureIndicesBufferView { get; }
        internal ShaderResourceView MaterialIndicesBufferView { get; }

        public IReadOnlyList<ModelMeshPart> MeshParts { get; }

        public uint NumTextureStages { get; }

        internal StaticBuffer<TexCoords> TexCoordVertexBuffer { get; }

        internal ModelMeshMaterialPass(
            ContentManager contentManager,
            ResourceUploadBatch uploadBatch,
            W3dMesh w3dMesh, 
            W3dMaterialPass w3dMaterialPass)
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
                    // TODO: What to do when this is null?
                    if (textureStage0.TexCoords != null)
                    {
                        texCoords[i].UV0 = textureStage0.TexCoords[i].ToVector2();
                    }

                    if (hasTextureStage1)
                    {
                        texCoords[i].UV1 = textureStage1.TexCoords[i].ToVector2();
                    }
                }
            }

            TexCoordVertexBuffer = AddDisposable(StaticBuffer.Create(
                contentManager.GraphicsDevice,
                uploadBatch,
                texCoords));

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

            var textureIndicesBuffer = AddDisposable(StaticBuffer.Create(
                contentManager.GraphicsDevice,
                uploadBatch,
                textureIDs));

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
                contentManager.GraphicsDevice,
                uploadBatch,
                materialIDs));

            TextureIndicesBufferView = AddDisposable(ShaderResourceView.Create(contentManager.GraphicsDevice, textureIndicesBuffer));
            MaterialIndicesBufferView = AddDisposable(ShaderResourceView.Create(contentManager.GraphicsDevice, materialIndicesBuffer));

            var meshParts = new List<ModelMeshPart>();

            if (w3dMaterialPass.ShaderIds.Length == 1)
            {
                meshParts.Add(new ModelMeshPart(
                    0, 
                    w3dMesh.Header.NumTris * 3, 
                    w3dMesh,
                    w3dMesh.Shaders[w3dMaterialPass.ShaderIds[0]]));
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
                            w3dMesh.Shaders[shaderID]));

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

        internal struct TexCoords
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
