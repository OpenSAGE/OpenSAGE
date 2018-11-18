using System.Numerics;

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

        public static Size CalculateSizeFittingAspectRatio(in SizeF size, in Size availableSize)
        {
            var rect = RectangleF.CalculateRectangleFittingAspectRatio(
                new RectangleF(Vector2.Zero, size),
                size,
                availableSize);

            return rect.Size;
        }
    }
}
