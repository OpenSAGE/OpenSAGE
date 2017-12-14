using System.Runtime.InteropServices;

namespace OpenSage.LowLevel.Graphics3D
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ColorRgbaF
    {
        public static readonly ColorRgbaF Transparent = new ColorRgbaF();

        public float R;
        public float G;
        public float B;
        public float A;

        public ColorRgbaF(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }
    }
}
