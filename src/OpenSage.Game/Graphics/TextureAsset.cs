using OpenSage.Data.StreamFS;
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

        internal TextureAsset(Texture texture, Asset asset)
        {
            SetNameAndInstanceId(asset);
            Texture = AddDisposable(texture);
        }

        public static implicit operator Texture(TextureAsset asset) => asset.Texture;
    }
}
