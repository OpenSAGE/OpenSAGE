using OpenSage.LowLevel.Graphics3D;
using OpenSage.Mathematics;

namespace OpenSage.Gui.Elements
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
