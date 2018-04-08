namespace OpenSage.Mathematics
{
    public struct ColorRgb
    {
        public byte R;
        public byte G;
        public byte B;

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
    }
}
