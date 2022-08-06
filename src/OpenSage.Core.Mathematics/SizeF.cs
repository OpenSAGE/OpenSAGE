using System;
using System.Numerics;

namespace OpenSage.Mathematics
{
    public readonly struct SizeF : IEquatable<SizeF>
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

        public override bool Equals(object obj)
        {
            return obj is SizeF && Equals((SizeF) obj);
        }

        public bool Equals(SizeF other)
        {
            return
                Width == other.Width &&
                Height == other.Height;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Width, Height);
        }

        public Vector2 ToVector2()
        {
            return new Vector2(Width, Height);
        }

        public static bool operator ==(in SizeF f1, in SizeF f2)
        {
            return f1.Equals(f2);
        }

        public static bool operator !=(in SizeF f1, in SizeF f2)
        {
            return !(f1 == f2);
        }
    }
}
