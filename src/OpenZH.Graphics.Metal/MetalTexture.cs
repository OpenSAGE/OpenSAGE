using Metal;

namespace OpenZH.Graphics.Metal
{
    public sealed class MetalTexture : Texture
    {
        public IMTLTexture Texture { get; }
    }
}