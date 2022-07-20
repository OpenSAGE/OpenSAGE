using Veldrid;

namespace OpenSage.Graphics
{
    public sealed class TextureAsset : BaseAsset
    {
        public Texture Texture { get; }

        internal TextureAsset(Texture texture, string name)
        {
            SetNameAndInstanceId("Texture", name);
            Texture = AddDisposable(texture);
        }

        public static implicit operator Texture(TextureAsset asset) => asset?.Texture;
    }
}
