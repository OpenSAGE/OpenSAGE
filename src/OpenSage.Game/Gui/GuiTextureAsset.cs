using Veldrid;

namespace OpenSage.Gui
{
    public sealed class GuiTextureAsset : BaseAsset
    {
        public Texture Texture { get; }

        internal GuiTextureAsset(Texture texture, string name)
        {
            SetNameAndInstanceId("GUITexture", name);
            Texture = AddDisposable(texture);
        }

        public static implicit operator Texture(GuiTextureAsset asset) => asset.Texture;
    }
}
