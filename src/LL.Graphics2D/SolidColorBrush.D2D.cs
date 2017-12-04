using LL.Graphics2D.Util;
using LL.Graphics3D;

namespace LL.Graphics2D
{
    partial class SolidColorBrush
    {
        private void PlatformConstruct(RenderTarget renderTarget, ColorRgbaF color)
        {
            DeviceBrush = AddDisposable(new SharpDX.Direct2D1.SolidColorBrush(
                renderTarget.DeviceRenderTarget,
                color.ToRawColor4()));
        }
    }
}
