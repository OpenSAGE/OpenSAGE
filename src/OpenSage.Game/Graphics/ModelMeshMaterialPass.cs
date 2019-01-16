using System.Collections.Generic;
using OpenSage.Graphics.Shaders;
using OpenSage.Utilities.Extensions;
using Veldrid;

namespace OpenSage.Graphics
{
    public sealed class ModelMeshMaterialPass : DisposableBase
    {
        public IReadOnlyList<ModelMeshPart> MeshParts { get; set; }

        internal DeviceBuffer TexCoordVertexBuffer;

        internal ModelMeshMaterialPass(
            GraphicsDevice graphicsDevice,
            MeshTypes.MeshVertex.TexCoords[] texCoords,
            IReadOnlyList<ModelMeshPart> meshParts)
        {
            if (texCoords != null)
            {
                TexCoordVertexBuffer = AddDisposable(graphicsDevice.CreateStaticBuffer(
                    texCoords,
                    BufferUsage.VertexBuffer));
            }

            MeshParts = meshParts;
        }
    }
}
