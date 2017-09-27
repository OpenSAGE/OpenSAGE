using LLGfx;

namespace OpenSage.Mathematics
{
    public struct Color
    {
        public static readonly Color White = new Color(255, 255, 255, 255);
        public static readonly Color DimGray = new Color(105, 105, 105, 255);

        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public Color(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public static Color operator*(Color value, float scale)
        {
            return new Color((byte) (value.R * scale), (byte) (value.G * scale), (byte) (value.B * scale), (byte) (value.A * scale));
        }

        public ColorRgba ToColorRgba()
        {
            return new ColorRgba(R / 255.0f, G / 255.0f, B / 255.0f, A / 255.0f);
        }
    }
}
