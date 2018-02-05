using Veldrid;
using Rectangle = OpenSage.Mathematics.Rectangle;

namespace OpenSage.Gui.Wnd
{
    public sealed class CroppedBitmap
    {
        public readonly Texture Bitmap;
        public readonly Rectangle SourceRect;

        public CroppedBitmap(Texture texture, in Rectangle sourceRect)
        {
            Bitmap = texture;
            SourceRect = sourceRect;
        }
    }
}
