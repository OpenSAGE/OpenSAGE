using System.Numerics;

namespace OpenSage.Mathematics
{
    public readonly struct ColorRgb
    {
        public readonly byte R;
        public readonly byte G;
        public readonly byte B;

        public ColorRgb(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        public static ColorRgb FromUInt32(uint rgb)
        {
            var r = (byte) (rgb >> 16 & 0xFF);
            var g = (byte) (rgb >> 8 & 0xFF);
            var b = (byte) (rgb & 0xFF);

            return new ColorRgb(r, g, b);
        }

        public ColorRgba ToColorRgba()
        {
            return new ColorRgba(R, G, B, 255);
        }

        public Vector3 ToVector3()
        {
            return new Vector3(R / 255.0f, G / 255.0f, B / 255.0f);
        }

        public ColorRgbF ToColorRgbF()
        {
            return new ColorRgbF(R / 255.0f, G / 255.0f, B / 255.0f);
        }

        public ColorRgbaF ToColorRgbaF()
        {
            return new ColorRgbaF(R / 255.0f, G / 255.0f, B / 255.0f, 1.0f);
        }
    }
}
