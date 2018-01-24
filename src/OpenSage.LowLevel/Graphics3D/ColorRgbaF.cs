﻿using System.Runtime.InteropServices;

namespace OpenSage.LowLevel.Graphics3D
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ColorRgbaF
    {
        public static readonly ColorRgbaF Transparent = new ColorRgbaF();
        public static readonly ColorRgbaF White = new ColorRgbaF(1.0f, 1.0f, 1.0f, 1.0f);

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

        public static ColorRgbaF operator*(in ColorRgbaF value1, in ColorRgbaF value2)
        {
            return new ColorRgbaF
            {
                R = value1.R * value2.R,
                G = value1.G * value2.G,
                B = value1.B * value2.B,
                A = value1.A * value2.A
            };
        }
    }
}
