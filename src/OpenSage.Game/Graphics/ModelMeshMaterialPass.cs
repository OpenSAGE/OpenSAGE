using System.Collections.Generic;
using OpenSage.Graphics.Shaders;
using OpenSage.Utilities.Extensions;
using Veldrid;

namespace OpenSage.Graphics
{
    public sealed class ModelMeshMaterialPass : DisposableBase
    {
        internal readonly List<ModelMeshPart> MeshParts;

        internal DeviceBuffer TexCoordVertexBuffer;

        internal ModelMeshMaterialPass(
            GraphicsDevice graphicsDevice,
            MeshTypes.MeshVertex.TexCoords[] texCoords,
            List<ModelMeshPart> meshParts)
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
