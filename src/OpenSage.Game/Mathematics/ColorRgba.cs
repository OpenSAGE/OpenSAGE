using System;

namespace OpenSage.Mathematics
{
    public readonly struct ColorRgba
    {
        public static readonly ColorRgba Transparent = new ColorRgba(255, 255, 255, 0);
        public static readonly ColorRgba White = new ColorRgba(255, 255, 255, 255);
        public static readonly ColorRgba DimGray = new ColorRgba(105, 105, 105, 255);

        public readonly byte R;
        public readonly byte G;
        public readonly byte B;
        public readonly byte A;

        public ColorRgba(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public static ColorRgba operator*(ColorRgba value, float scale)
        {
            return new ColorRgba((byte) (value.R * scale), (byte) (value.G * scale), (byte) (value.B * scale), (byte) (value.A * scale));
        }

        public ColorRgbaF ToColorRgbaF()
        {
            return new ColorRgbaF(R / 255.0f, G / 255.0f, B / 255.0f, A / 255.0f);
        }

        public string ToHex()
        {
            byte[] data = { R,G,B,A };

            string hex = BitConverter.ToString(data).Replace("-", string.Empty);
            return hex;
        }

        public static ColorRgba FromHex(in ColorRgba original, string hexString)
        {
            var hexVal = Convert.ToUInt32(hexString, 16);
            bool hasAlpha = hexString.Length > 8;

            var a = original.A;

            var b = (byte) (hexVal        & 0xFF);
            var g = (byte) (hexVal >>   8 & 0xFF);
            var r = (byte) (hexVal >>  16 & 0xFF);

            if (hasAlpha)
            {
                a = (byte) (hexVal >> 24 & 0xFF);
            }

            return new ColorRgba(r, g, b, a);
        }
    }
}
