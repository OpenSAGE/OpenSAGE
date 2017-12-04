using LL.Graphics2D.Util;
using LL.Graphics3D;
using D2D1RenderTarget = SharpDX.Direct2D1.RenderTarget;

namespace LL.Graphics2D
{
    partial class DrawingContext
    {
        private D2D1RenderTarget _deviceRenderTarget;

        private void PlatformConstruct(RenderTarget renderTarget)
        {
            _deviceRenderTarget = renderTarget.DeviceRenderTarget;

            _deviceRenderTarget.BeginDraw();
        }

        private void PlatformClear(ColorRgbaF clearColor)
        {
            _deviceRenderTarget.Clear(clearColor.ToRawColor4());
        }

        private void PlatformFillRectangle(RawRectangle rect, Brush brush)
        {
            _deviceRenderTarget.FillRectangle(
                rect.ToRawRectangleF(),
                brush.DeviceBrush);
        }

        private void PlatformClose()
        {
            _deviceRenderTarget.EndDraw();
        }
    }
}
