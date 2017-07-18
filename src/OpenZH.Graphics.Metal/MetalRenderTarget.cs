using Metal;

namespace OpenZH.Graphics.Metal
{
    public sealed class MetalRenderTarget : RenderTarget
    {
        public IMTLTexture Texture { get; }

        public MetalRenderTarget(IMTLTexture texture)
        {
            Texture = texture;
        }
    }
}