using Metal;

namespace OpenZH.Graphics.Metal
{
    public sealed class MetalRenderTargetView : RenderTargetView
    {
        public IMTLTexture Texture { get; }

        public MetalRenderTargetView(IMTLTexture texture)
        {
            Texture = texture;
        }
    }
}