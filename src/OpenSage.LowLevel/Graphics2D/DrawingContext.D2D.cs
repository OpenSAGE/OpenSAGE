using OpenSage.LowLevel.Graphics3D;
using OpenSage.LowLevel.Graphics3D.Util;
using SharpDX.Direct2D1;
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
        }

        private void PlatformBegin()
        {
            _graphicsDevice.DeviceContext.Target = _bitmap;

            _graphicsDevice.DeviceContext.BeginDraw();
        }

        private void PlatformDrawImage(Texture image, in RawRectangleF sourceRect, in RawRectangleF destinationRect, bool interpolate)
        {
            using (var dxgiSurface = image.DeviceResource.QueryInterface<Surface>())
            using (var bitmap = new Bitmap1(_graphicsDevice.DeviceContext, dxgiSurface))
            {
                _graphicsDevice.DeviceContext.DrawBitmap(
                    bitmap,
                    ToRawRectangleF(destinationRect),
                    1.0f,
                    interpolate ? InterpolationMode.HighQualityCubic : InterpolationMode.NearestNeighbor,
                    ToRawRectangleF(sourceRect),
                    null);
            }
        }

        private void PlatformDrawText(string text, TextFormat textFormat, in ColorRgbaF color, in RawRectangleF rect)
        {
            using (var foregroundBrush = CreateBrush(color))
            {
                _graphicsDevice.DeviceContext.DrawText(
                    text,
                    textFormat.DeviceTextFormat,
                    ToRawRectangleF(rect),
                    foregroundBrush,
                    DrawTextOptions.None,
                    MeasuringMode.Natural);
            }
        }

        private void PlatformClear(in ColorRgbaF color)
        {
            using (var brush = CreateBrush(color))
            {
                _graphicsDevice.DeviceContext.Clear(color.ToRawColor4());
            }
        }

        private void PlatformDrawRectangle(in RawRectangleF rect, in ColorRgbaF strokeColor, float strokeWidth)
        {
            using (var brush = CreateBrush(strokeColor))
            {
                _graphicsDevice.DeviceContext.DrawRectangle(
                    ToRawRectangleF(rect),
                    brush,
                    strokeWidth);
            }
        }

        private SolidColorBrush CreateBrush(in ColorRgbaF color)
        {
            return new SolidColorBrush(_graphicsDevice.DeviceContext, color.ToRawColor4());
        }

        private static SharpDX.Mathematics.Interop.RawRectangleF ToRawRectangleF(in RawRectangleF value)
        {
            return new SharpDX.Mathematics.Interop.RawRectangleF(value.X, value.Y, value.X + value.Width, value.Y + value.Height);
        }

        private void PlatformEnd()
        {
            _graphicsDevice.DeviceContext.EndDraw();

            _graphicsDevice.DeviceContext.Target = null;
        }
    }
}
