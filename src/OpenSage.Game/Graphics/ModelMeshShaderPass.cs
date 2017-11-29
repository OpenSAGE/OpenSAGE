using LLGfx;
using LLGfx.Effects;
using System.Numerics;

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
