using LLGfx;
using LLGfx.Effects;
using System.Numerics;

namespace OpenSage.Graphics
{
    public sealed class ModelMeshShaderPass : DisposableBase
    {
        internal StaticBuffer<Vector2> TexCoordVertexBuffer;

        internal ModelMeshShaderPass(
            GraphicsDevice graphicsDevice,
            Vector2[] texCoords,
            Effect effect)
        {
            TexCoordVertexBuffer = AddDisposable(StaticBuffer.Create(
                graphicsDevice,
                texCoords,
                BufferBindFlags.VertexBuffer));
        }
    }
}
