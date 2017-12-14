using System.Collections.Generic;
using OpenSage.LowLevel.Graphics3D;

namespace OpenSage.Graphics
{
    public sealed class ModelMeshMaterialPass : DisposableBase
    {
        public IReadOnlyList<ModelMeshPart> MeshParts { get; set; }

        internal Buffer<MeshVertex.TexCoords> TexCoordVertexBuffer;

        internal ModelMeshMaterialPass(
            GraphicsDevice graphicsDevice,
            MeshVertex.TexCoords[] texCoords,
            IReadOnlyList<ModelMeshPart> meshParts)
        {
            TexCoordVertexBuffer = AddDisposable(Buffer<MeshVertex.TexCoords>.CreateStatic(
                graphicsDevice,
                texCoords,
                BufferBindFlags.VertexBuffer));

            MeshParts = meshParts;
        }
    }
}
