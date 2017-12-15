using OpenSage.LowLevel.Graphics3D;
using OpenSage.LowLevel.Graphics3D.Util;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.DXGI;

namespace OpenSage.LowLevel.Graphics2D
{
    partial class DrawingContext
    {
        private GraphicsDevice2D _graphicsDevice;
        private Bitmap1 _bitmap;

        private void PlatformConstruct(GraphicsDevice2D graphicsDevice, Texture targetTexture)
        {
            _graphicsDevice = graphicsDevice;

            var dxgiSurface = targetTexture.DeviceResource.QueryInterface<Surface>();

            _bitmap = AddDisposable(new Bitmap1(graphicsDevice.DeviceContext, dxgiSurface));

            graphicsDevice.DeviceContext.Target = _bitmap;

            graphicsDevice.DeviceContext.BeginDraw();
        }

        private void PlatformDrawText(string text, TextFormat textFormat, in ColorRgbaF color, in RawRectangleF rect)
        {
            using (var foregroundBrush = new SolidColorBrush(_graphicsDevice.DeviceContext, color.ToRawColor4()))
            {
                _graphicsDevice.DeviceContext.DrawText(
                    text,
                    textFormat.DeviceTextFormat,
                    new SharpDX.Mathematics.Interop.RawRectangleF(rect.X, rect.Y, rect.X + rect.Width, rect.Y + rect.Height),
                    foregroundBrush,
                    DrawTextOptions.None,
                    MeasuringMode.Natural);
            }
        }

        private void PlatformClose()
        {
            _graphicsDevice.DeviceContext.EndDraw();

            _graphicsDevice.DeviceContext.Target = null;
        }
    }
}
