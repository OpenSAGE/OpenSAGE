using OpenSage.Data.W3d;

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

        public W3dRgb ToW3dRgb()
        {
            return new W3dRgb
            {
                R = R,
                G = G,
                B = B
            };
        }
    }
}
