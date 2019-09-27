using OpenSage.Content;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Gui.Wnd.Images
{
    internal abstract class ImageSource
    {
        public abstract Size NaturalSize { get; }

        public abstract Texture GetTexture(Size size);
    }
}
