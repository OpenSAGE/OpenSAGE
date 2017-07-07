namespace OpenZH.Data.Map
{
    public struct ColorArgb
    {
        public byte A;
        public byte R;
        public byte G;
        public byte B;

        public ColorArgb(byte a, byte r, byte g, byte b)
        {
            A = a;
            R = r;
            G = g;
            B = b;
        }

        public override bool Equals(object obj)
        {
            return (obj is ColorArgb) && Equals((ColorArgb) obj);
        }

        public bool Equals(ColorArgb other)
        {
            return A == other.A
                && R == other.R
                && G == other.G
                && B == other.B;
        }

        public override string ToString()
        {
            return $"{{A:{A} R:{R} G:{G} B:{B}}}";
        }
    }
}
