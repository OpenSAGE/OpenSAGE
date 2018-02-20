using Veldrid;
using Rectangle = OpenSage.Mathematics.Rectangle;

namespace OpenSage.Gui.Wnd
{
    internal sealed class MappedImageTexture
    {
        public readonly Texture Texture;
        public readonly Rectangle SourceRect;

        public MappedImageTexture(Texture texture, in Rectangle sourceRect)
        {
            Texture = texture;
            SourceRect = sourceRect;
        }
    }
}
