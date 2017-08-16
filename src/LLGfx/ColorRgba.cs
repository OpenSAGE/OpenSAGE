namespace LLGfx
{
    public struct ColorRgba
    {
        public static readonly ColorRgba Transparent = new ColorRgba();

        public float R;
        public float G;
        public float B;
        public float A;

        public ColorRgba(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }
    }
}
