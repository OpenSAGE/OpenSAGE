using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Gui.Wnd.Images
{
    internal sealed class TextureSource : ImageSource
    {
        private readonly Texture _texture;

        public override Size NaturalSize => new Size((int) _texture.Width, (int) _texture.Height);

        public TextureSource(Texture texture)
        {
            _texture = texture;
        }

        public override Texture GetTexture(Size size) => _texture;
    }
}
