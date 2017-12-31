using OpenSage.LowLevel.Graphics3D;
using OpenSage.LowLevel.Graphics3D.Util;
using SharpDX.Direct2D1;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;

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

        private void PlatformDrawLine(in RawLineF line, in ColorRgbaF strokeColor)
        {
            using (var brush = CreateBrush(strokeColor))
            {
                _graphicsDevice.DeviceContext.DrawLine(
                    ToRawVector2(line.X1, line.Y1),
                    ToRawVector2(line.X2, line.Y2),
                    brush,
                    line.Thickness);
            }
        }

        private void PlatformFillRectangle(in RawRectangleF rect, in ColorRgbaF fillColor)
        {
            using (var brush = CreateBrush(fillColor))
            {
                _graphicsDevice.DeviceContext.FillRectangle(
                    ToRawRectangleF(rect),
                    brush);
            }
        }

        private void PlatformFillTriangle(in RawTriangleF tri, in ColorRgbaF fillColor)
        {
            using (var brush = CreateBrush(fillColor))
            {
                var geometry = new PathGeometry(_graphicsDevice.DeviceContext.Factory);
                var sink = geometry.Open();
                sink.BeginFigure(ToRawVector2(tri.X1, tri.Y1),FigureBegin.Filled);
                sink.AddLine(ToRawVector2(tri.X2, tri.Y2));
                sink.AddLine(ToRawVector2(tri.X3, tri.Y3));
                sink.EndFigure(FigureEnd.Closed);
                sink.Close();
                _graphicsDevice.DeviceContext.FillGeometry(geometry, brush);
            }
        }

        private SolidColorBrush CreateBrush(in ColorRgbaF color)
        {
            
            return new SolidColorBrush(_graphicsDevice.DeviceContext, color.ToRawColor4());
        }

        private ImageBrush CreateImageBrush(in Image img,RawMatrix3x2 transform)
        {
            var prop = new ImageBrushProperties()
            {
                ExtendModeX = ExtendMode.Clamp,
                ExtendModeY = ExtendMode.Clamp,
                InterpolationMode = InterpolationMode.NearestNeighbor
            };

            var brush = new ImageBrush(_graphicsDevice.DeviceContext, img, prop);

            brush.Transform = transform;

            return brush;
        }

        private static SharpDX.Mathematics.Interop.RawRectangleF ToRawRectangleF(in RawRectangleF value)
        {
            return new SharpDX.Mathematics.Interop.RawRectangleF(value.X, value.Y, value.X + value.Width, value.Y + value.Height);
        }

        private static SharpDX.Mathematics.Interop.RawVector2 ToRawVector2(in float x,in float y)
        {
            return new SharpDX.Mathematics.Interop.RawVector2(x,y);
        }

        private void PlatformEnd()
        {
            _graphicsDevice.DeviceContext.EndDraw();

            _graphicsDevice.DeviceContext.Target = null;
        }
    }
}
