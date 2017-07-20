using Metal;

namespace OpenZH.Graphics
{
    partial class RenderTarget
    {
        internal IMTLTexture Texture { get; }

        internal RenderTarget(GraphicsDevice graphicsDevice, IMTLTexture texture)
            : base(graphicsDevice)
        {
            Texture = texture;
        }
    }
}