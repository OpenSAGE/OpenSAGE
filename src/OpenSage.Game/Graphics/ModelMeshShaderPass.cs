using OpenSage.LowLevel.Graphics3D;
using System.Numerics;
using OpenSage.Graphics.Effects;

namespace OpenSage.Graphics
{
    public sealed class ModelMeshShaderPass : DisposableBase
    {
        internal Buffer<Vector2> TexCoordVertexBuffer;

        internal ModelMeshShaderPass(
            GraphicsDevice graphicsDevice,
            Vector2[] texCoords,
            Effect effect)
        {
            TexCoordVertexBuffer = AddDisposable(Buffer<Vector2>.CreateStatic(
                graphicsDevice,
                texCoords,
                BufferBindFlags.VertexBuffer));
        }
    }
}
