using System.Collections.Generic;
using System.Numerics;
using LLGfx;

namespace OpenSage.Graphics
{
    public sealed class ModelMeshMaterialPass : DisposableBase
    {
        public uint NumTextureStages { get; set; }

        public IReadOnlyList<ModelMeshPart> MeshParts { get; set; }

        internal StaticBuffer<MeshTexCoords> TexCoordVertexBuffer;
        internal StaticBuffer<uint> MaterialIndicesBuffer;

        internal ModelMeshMaterialPass(
            GraphicsDevice graphicsDevice,
            uint numTextureStages,
            MeshTexCoords[] texCoords,
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

    public struct MeshTexCoords
    {
        public Vector2 UV0;
        public Vector2 UV1;
    }
}
