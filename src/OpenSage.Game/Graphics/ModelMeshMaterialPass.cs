using System.Collections.Generic;
using LLGfx;

namespace OpenSage.Graphics
{
    public sealed class ModelMeshMaterialPass : DisposableBase
    {
        public uint NumTextureStages { get; set; }

        public IReadOnlyList<ModelMeshPart> MeshParts { get; set; }

        internal StaticBuffer<MeshVertex.TexCoords> TexCoordVertexBuffer;
        internal StaticBuffer<uint> MaterialIndicesBuffer;

        internal ModelMeshMaterialPass(
            GraphicsDevice graphicsDevice,
            uint numTextureStages,
            MeshVertex.TexCoords[] texCoords,
            uint[] materialIndices,
            IReadOnlyList<ModelMeshPart> meshParts)
        {
            NumTextureStages = numTextureStages;

            TexCoordVertexBuffer = AddDisposable(StaticBuffer.Create(
                graphicsDevice,
                texCoords,
                BufferBindFlags.VertexBuffer));

            MaterialIndicesBuffer = AddDisposable(StaticBuffer.Create(
                graphicsDevice,
                materialIndices,
                BufferBindFlags.ShaderResource));

            MeshParts = meshParts;
        }
    }
}
