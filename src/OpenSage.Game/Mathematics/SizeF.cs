using System.Numerics;

namespace OpenSage.Mathematics
{
    public readonly struct SizeF
    {
        public static readonly SizeF Zero = new SizeF(0, 0);

        public readonly float Width;
        public readonly float Height;

        public SizeF(float size) : this(size, size) { }

        public SizeF(float width, float height)
        {
            Width = width;
            Height = height;
        }

        public static Size CalculateSizeFittingAspectRatio(in SizeF size, in Size availableSize)
        {
            var rect = RectangleF.CalculateRectangleFittingAspectRatio(
                new RectangleF(Vector2.Zero, size),
                size,
                availableSize);

            return rect.Size;
        }

        public Vector2 ToVector2()
        {
            return new Vector2(Width, Height);
        }
    }
}
