using LL.Graphics3D;
using SharpDX.Mathematics.Interop;

namespace LL.Graphics2D.Util
{
    internal static class ConversionExtensions
    {
        public static RawRectangleF ToRawRectangleF(this RawRectangle rect)
        {
            return new RawRectangleF(rect.X, rect.Y, rect.X + rect.Width, rect.Y + rect.Height);
        }

        public static RawColor4 ToRawColor4(this ColorRgbaF color)
        {
            return new RawColor4(color.R, color.G, color.B, color.A);
        }
    }
}
