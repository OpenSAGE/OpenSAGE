using System.Runtime.InteropServices;
using Veldrid;

namespace OpenSage.Mathematics
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct ColorRgbaF
    {
        public static readonly ColorRgbaF Transparent = new ColorRgbaF();
        public static readonly ColorRgbaF White = new ColorRgbaF(1.0f, 1.0f, 1.0f, 1.0f);
        public static readonly ColorRgbaF Black = new ColorRgbaF(0.0f, 0.0f, 0.0f, 1.0f);

        public readonly float R;
        public readonly float G;
        public readonly float B;
        public readonly float A;

        public ColorRgbaF(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public static ColorRgbaF operator*(in ColorRgbaF value1, in ColorRgbaF value2)
        {
            return new ColorRgbaF(
                value1.R * value2.R,
                value1.G * value2.G,
                value1.B * value2.B,
                value1.A * value2.A);
        }

        public RgbaFloat ToRgbaFloat()
        {
            return new RgbaFloat(R, G, B, A);
        }

        public ColorRgbaF WithA(float a)
        {
            return new ColorRgbaF(R, G, B, a);
        }
    }
}
