namespace OpenSage.LowLevel.Graphics2D
{
    public struct RawRectangleF
    {
        public float X;
        public float Y;
        public float Width;
        public float Height;

        public float Right => X + Width;

        public RawRectangleF(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }
}
