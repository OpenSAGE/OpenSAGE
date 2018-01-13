using System.Numerics;
using OpenSage.LowLevel.Graphics3D;

namespace OpenSage.LowLevel.Graphics2D
{
    partial class DrawingContext
    {
        private void PlatformConstruct(GraphicsDevice2D graphicsDevice, Texture targetTexture)
        {
            
        }

        private void PlatformBegin()
        {

        }

        private void PlatformClear(in ColorRgbaF color) { }

        private void PlatformDrawImage(Texture image, in RawRectangleF sourceRect, in RawRectangleF destinationRect, bool interpolate)
        {

        }

        private void PlatformDrawText(string text, TextFormat textFormat, in ColorRgbaF color, in RawRectangleF rect)
        {
            
        }
    
        private void PlatformDrawRectangle(in RawRectangleF rect, in ColorRgbaF strokeColor, float strokeWidth)
        {

        }

        private void PlatformDrawLine(in RawLineF line, in ColorRgbaF strokeColor)
        {

        }

        private void PlatformFillTriangle(in RawTriangleF tri, in ColorRgbaF fillColor)
        {

        }

        private void PlatformFillTriangle(in RawTriangleF tri, in Texture tex, in Matrix3x2 mat)
        {

        }

        private void PlatformColorTransform(in ColorRgbaF color)
        {

        }

        private void PlatformFillRectangle(in RawRectangleF rect, in ColorRgbaF fillColor)
        {

        }

        private void PlatformTransform(in Matrix3x2 matrix)
        {

        }

        private void PlatformEnd()
        {
            
        }
    }
}
