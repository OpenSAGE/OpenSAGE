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
            ResourceUploadBatch uploadBatch,
            Vector2[] texCoords,
            Effect effect)
        {
            TexCoordVertexBuffer = AddDisposable(StaticBuffer.Create(
                graphicsDevice,
                uploadBatch,
                texCoords));
        }
    }
}
