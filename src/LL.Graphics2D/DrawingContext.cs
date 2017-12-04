using LL.Graphics3D;

namespace LL.Graphics2D
{
    public sealed partial class DrawingContext
    {
        internal DrawingContext(RenderTarget renderTarget)
        {
            PlatformConstruct(renderTarget);
        }

        public void DrawText(string text)
        {
            //throw new System.NotImplementedException();
        }

        public void Clear(ColorRgbaF clearColor)
        {
            PlatformClear(clearColor);
        }

        public void FillRectangle(RawRectangle rect, Brush brush)
        {
            PlatformFillRectangle(rect, brush);
        }

        public void Close()
        {
            PlatformClose();
        }
    }
}
