using OpenSage.LowLevel.Graphics3D;

namespace OpenSage.LowLevel.Graphics2D
{
    public sealed partial class DrawingContext : DisposableBase
    {
        public DrawingContext(GraphicsDevice2D graphicsDevice, Texture targetTexture)
        {
            PlatformConstruct(graphicsDevice, targetTexture);
        }

        public void DrawText(string text, TextFormat textFormat, in ColorRgbaF color, in RawRectangleF rect)
        {
            PlatformDrawText(text, textFormat, color, rect);
        }

        public void Close()
        {
            PlatformClose();
        }
    }
}
