using System;
using System.Numerics;

namespace OpenSage.Mathematics
{
    /// <summary>
    /// Describes a floating-point rectangle.
    /// </summary>
    public readonly struct RectangleF
    {
        /// <summary>
        /// Gets the x component of the <see cref="RectangleF"/>.
        /// </summary>
        public readonly float X;

        /// <summary>
        /// Gets the x component of the <see cref="RectangleF"/>.
        /// </summary>
        public readonly float Y;

        /// <summary>
        /// Gets the width of the <see cref="RectangleF"/>.
        /// </summary>
        public readonly float Width;

        /// <summary>
        /// Gets the height of the <see cref="RectangleF"/>.
        /// </summary>
        public readonly float Height;

        public Vector2 Position => new Vector2(X, Y);
        public SizeF Size => new SizeF(Width, Height);

        public float Left => X;
        public float Right => X + Width;
        public float Top => Y;
        public float Bottom => Y + Height;

        /// <summary>
        /// Creates a new <see cref="RectangleF"/>.
        /// </summary>
        public RectangleF(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public RectangleF(in Vector2 position, in SizeF size)
        {
            X = position.X;
            Y = position.Y;
            Width = size.Width;
            Height = size.Height;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{{X:{X} Y:{Y} Width:{Width} Height:{Height}}}";
        }

        public static Rectangle CalculateRectangleFittingAspectRatio(
            in RectangleF rect,
            in SizeF boundsSize,
            in Size viewportSize)
        {
            return CalculateRectangleFittingAspectRatio(rect, boundsSize, viewportSize, out _);
        }

        public static Rectangle CalculateRectangleFittingAspectRatio(
            in RectangleF rect,
            in SizeF boundsSize,
            in Size viewportSize,
            out float scale)
        {
            // Figure out the ratio.
            var ratioX = viewportSize.Width / boundsSize.Width;
            var ratioY = viewportSize.Height / boundsSize.Height;

            // Use whichever multiplier is smaller.
            var ratio = ratioX < ratioY ? ratioX : ratioY;

            scale = ratio;

            // Now we can get the new height and width
            var newWidth = (int) Math.Round(rect.Width * ratio);
            var newHeight = (int) Math.Round(rect.Height * ratio);

            newWidth = Math.Max(newWidth, 1);
            newHeight = Math.Max(newHeight, 1);

            var newX = (int) Math.Round(rect.X * ratio);
            var newY = (int) Math.Round(rect.Y * ratio);

            // Now calculate the X,Y position of the upper-left corner 
            // (one of these will always be zero for the top level window)
            var posX = (int) Math.Round((viewportSize.Width - (boundsSize.Width * ratio)) / 2.0) + newX;
            var posY = (int) Math.Round((viewportSize.Height - (boundsSize.Height * ratio)) / 2.0) + newY;

            return new Rectangle(posX, posY, newWidth, newHeight);
        }

        public static Matrix3x2 CalculateTransformForRectangleFittingAspectRatio(
            in RectangleF rect,
            in SizeF boundsSize,
            in Size viewportSize)
        {
            // Figure out the ratio.
            var ratioX = viewportSize.Width / boundsSize.Width;
            var ratioY = viewportSize.Height / boundsSize.Height;

            // Use whichever multiplier is smaller.
            var ratio = ratioX < ratioY ? ratioX : ratioY;

            var newX = (int) Math.Round(rect.X * ratio);
            var newY = (int) Math.Round(rect.Y * ratio);

            // Now calculate the X,Y position of the upper-left corner 
            // (one of these will always be zero for the top level window)
            var posX = (int) Math.Round((viewportSize.Width - (boundsSize.Width * ratio)) / 2.0) + newX;
            var posY = (int) Math.Round((viewportSize.Height - (boundsSize.Height * ratio)) / 2.0) + newY;

            return
                Matrix3x2.CreateScale(ratio) *
                Matrix3x2.CreateTranslation(posX, posY);
        }

        public static RectangleF Transform(in RectangleF rect, in Matrix3x2 matrix)
        {
            var position = Vector2.Transform(new Vector2(rect.X, rect.Y), matrix);
            var size = Vector2.TransformNormal(new Vector2(rect.Width, rect.Height), matrix);

            return new RectangleF(position.X, position.Y, size.X, size.Y);
        }

        public bool Contains(in Point2D point)
        {
            return point.X >= X
                && point.X <= Right
                && point.Y >= Y
                && point.Y <= Bottom;
        }

        public bool Contains(Vector2 point)
        {
            return point.X >= X
                && point.X <= Right
                && point.Y >= Y
                && point.Y <= Bottom;
        }

        public bool Contains(in RectangleF rect)
        {
            return rect.Left >= Left
                && rect.Right <= Right
                && rect.Top >= Top
                && rect.Bottom <= Bottom;
        }

        public bool IntersectsWith(in RectangleF rect)
        {
            return rect.Left <= Right
                && rect.Right >= Left
                && rect.Top <= Bottom
                && rect.Bottom >= Top;
        }

        // TODO: It might make sense to micro-optimise this, as it's a very common operation.
        public ContainmentType Intersect(in RectangleF rect)
        {
            if (Contains(rect))
            {
                return ContainmentType.Contains;
            }

            if (IntersectsWith(rect))
            {
                return ContainmentType.Intersects;
            }

            return ContainmentType.Disjoint;
        }

        public RectangleF WithY(float y)
        {
            return new RectangleF(X, y, Width, Height);
        }

        public bool Equals(in RectangleF other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y) && Width.Equals(other.Width) && Height.Equals(other.Height);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is RectangleF other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X.GetHashCode(), Y.GetHashCode(), Width.GetHashCode(), Height.GetHashCode());
        }
    }
}
