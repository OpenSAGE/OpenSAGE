namespace OpenSage.DataViewer.Framework.BlockCompression
{
    internal struct ColorRgba
    {
        public const int SizeInBytes = 4;

        public static readonly ColorRgba TransparentBlack = new ColorRgba();

        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public static ColorRgba UnpackRgbFromBgr565(ushort packedValue)
        {
            var r = (byte) ((packedValue >> 11) & 0b11111); // 5 bits
            var g = (byte) ((packedValue >> 5) & 0b111111); // 6 bits
            var b = (byte) (packedValue & 0x1F);            // 5 bits

            return new ColorRgba
            {
                R = (byte) (((r * 255) + 15) / 31.0),
                G = (byte) (((g * 255) + 31) / 63.0),
                B = (byte) (((b * 255) + 15) / 31.0),
                A = 255
            };
        }

        public static ColorRgba operator *(double multiplier, ColorRgba color)
        {
            return new ColorRgba
            {
                R = (byte) (color.R * multiplier),
                G = (byte) (color.G * multiplier),
                B = (byte) (color.B * multiplier),
                A = 255
            };
        }

        public static ColorRgba operator +(ColorRgba value1, ColorRgba value2)
        {
            return new ColorRgba
            {
                R = (byte) (value1.R + value2.R),
                G = (byte) (value1.G + value2.G),
                B = (byte) (value1.B + value2.B),
                A = 255
            };
        }
    }
}
