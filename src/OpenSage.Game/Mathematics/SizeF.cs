namespace OpenSage.Mathematics
{
    public struct SizeF
    {
        public static readonly SizeF Zero = new SizeF(0, 0);

        public float Width;
        public float Height;

        public SizeF(float width, float height)
        {
            Width = width;
            Height = height;
        }
    }
}
