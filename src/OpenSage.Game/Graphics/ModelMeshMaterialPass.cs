using System.Collections.Generic;
using LLGfx;

namespace OpenSage.Graphics
{
    public sealed class ModelMeshMaterialPass : DisposableBase
    {
        public uint NumTextureStages { get; set; }

        public IReadOnlyList<ModelMeshPart> MeshParts { get; set; }

        internal Buffer<MeshVertex.TexCoords> TexCoordVertexBuffer;
        internal Buffer<uint> MaterialIndicesBuffer;

        internal ModelMeshMaterialPass(
            GraphicsDevice graphicsDevice,
            uint numTextureStages,
            MeshVertex.TexCoords[] texCoords,
            uint[] materialIndices,
            IReadOnlyList<ModelMeshPart> meshParts)
        {
            NumTextureStages = numTextureStages;

            TexCoordVertexBuffer = AddDisposable(Buffer<MeshVertex.TexCoords>.CreateStatic(
                graphicsDevice,
                texCoords,
                BufferBindFlags.VertexBuffer));

            MaterialIndicesBuffer = AddDisposable(Buffer<uint>.CreateStatic(
                graphicsDevice,
                materialIndices,
                BufferBindFlags.ShaderResource));

            MeshParts = meshParts;
        }
    }
}
