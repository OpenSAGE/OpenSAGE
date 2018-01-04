using System.IO;
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

            using (var dxgiSurface = targetTexture.DeviceResource.QueryInterface<Surface>())
            {
                _bitmap = AddDisposable(new Bitmap1(graphicsDevice.DeviceContext, dxgiSurface));
            }
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
            using (var geometry = new PathGeometry(_graphicsDevice.DeviceContext.Factory))
            {
                using (var sink = geometry.Open())
                {
                    sink.BeginFigure(ToRawVector2(tri.X1, tri.Y1), FigureBegin.Filled);
                    sink.SetFillMode(SharpDX.Direct2D1.FillMode.Winding);
                    sink.AddLine(ToRawVector2(tri.X2, tri.Y2));
                    sink.AddLine(ToRawVector2(tri.X3, tri.Y3));
                    sink.EndFigure(FigureEnd.Closed);
                    sink.Close();
                }

                _graphicsDevice.DeviceContext.FillGeometry(geometry, brush);
            }
        }

        private void PlatformFillTriangle(in RawTriangleF triangle, Texture texture, in RawMatrix3x2 transform)
        {
            if (texture.MipMapCount > 1)
            {
                throw new InvalidDataException("Texture is not allowed to have Mipmaps for Direct2D!");
            }

            using (var dxgiSurface = texture.DeviceResource.QueryInterface<Surface>())
            using (var bitmap = new Bitmap1(_graphicsDevice.DeviceContext, dxgiSurface))
            using (var brush = CreateImageBrush(bitmap, transform))
            using (var geometry = new PathGeometry(_graphicsDevice.DeviceContext.Factory))
            {
                using (var sink = geometry.Open())
                {
                    sink.SetFillMode(SharpDX.Direct2D1.FillMode.Winding);
                    sink.BeginFigure(ToRawVector2(triangle.X1, triangle.Y1), FigureBegin.Filled);
                    sink.AddLine(ToRawVector2(triangle.X2, triangle.Y2));
                    sink.AddLine(ToRawVector2(triangle.X3, triangle.Y3));
                    sink.EndFigure(FigureEnd.Closed);
                    sink.Close();
                }

                _graphicsDevice.DeviceContext.FillGeometry(geometry, brush);
            }
        }

        private SolidColorBrush CreateBrush(in ColorRgbaF color)
        {
            return new SolidColorBrush(_graphicsDevice.DeviceContext, color.ToRawColor4());
        }

        private BitmapBrush1 CreateImageBrush(in Bitmap1 image, in RawMatrix3x2 transform)
        {
            var brushProp = new BrushProperties()
            {
                Transform = ToRawMatrix3x2(transform),
                Opacity = 1.0f
            };

            return new BitmapBrush1(_graphicsDevice.DeviceContext, image, brushProp);
        }

        private static SharpDX.Mathematics.Interop.RawRectangleF ToRawRectangleF(in RawRectangleF value)
        {
            return new SharpDX.Mathematics.Interop.RawRectangleF(value.X, value.Y, value.X + value.Width, value.Y + value.Height);
        }

        private static SharpDX.Mathematics.Interop.RawMatrix3x2 ToRawMatrix3x2(in RawMatrix3x2 m)
        {
            return new SharpDX.Mathematics.Interop.RawMatrix3x2(m.M11, m.M12, m.M21, m.M22, m.M31, m.M32);
        }

        private static SharpDX.Mathematics.Interop.RawVector2 ToRawVector2(float x, float y)
        {
            return new SharpDX.Mathematics.Interop.RawVector2(x, y);
        }

        private void PlatformEnd()
        {
            _graphicsDevice.DeviceContext.EndDraw();

            _graphicsDevice.DeviceContext.Target = null;
        }
    }
}
