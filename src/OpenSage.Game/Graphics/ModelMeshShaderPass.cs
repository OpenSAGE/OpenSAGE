using System.Numerics;
using OpenSage.Graphics.Effects;
using OpenSage.Utilities.Extensions;
using Veldrid;

namespace OpenSage.Graphics
{
    public sealed class ModelMeshShaderPass : DisposableBase
    {
        internal DeviceBuffer TexCoordVertexBuffer;

        internal ModelMeshShaderPass(
            GraphicsDevice graphicsDevice,
            Vector2[] texCoords,
            Effect effect)
        {
            TexCoordVertexBuffer = AddDisposable(graphicsDevice.CreateStaticBuffer(
                texCoords,
                BufferUsage.VertexBuffer));
        }
    }
}
