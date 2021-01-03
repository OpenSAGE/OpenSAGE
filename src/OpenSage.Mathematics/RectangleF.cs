using System;
using System.Numerics;

namespace OpenSage.Mathematics
{
    /// <summary>
    /// Describes a axis-aligned floating-point rectangle.
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

        public RectangleF(in Vector2 position, float width, float height)
        {
            X = position.X;
            Y = position.Y;
            Width = width;
            Height = height;
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
            var newWidth = (int) MathF.Round(rect.Width * ratio);
            var newHeight = (int) MathF.Round(rect.Height * ratio);

            newWidth = Math.Max(newWidth, 1);
            newHeight = Math.Max(newHeight, 1);

            var newX = (int) MathF.Round(rect.X * ratio);
            var newY = (int) MathF.Round(rect.Y * ratio);

            // Now calculate the X,Y position of the upper-left corner 
            // (one of these will always be zero for the top level window)
            var posX = (int) MathF.Round((viewportSize.Width - (boundsSize.Width * ratio)) / 2.0f) + newX;
            var posY = (int) MathF.Round((viewportSize.Height - (boundsSize.Height * ratio)) / 2.0f) + newY;

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

            var newX = (int) MathF.Round(rect.X * ratio);
            var newY = (int) MathF.Round(rect.Y * ratio);

            // Now calculate the X,Y position of the upper-left corner 
            // (one of these will always be zero for the top level window)
            var posX = (int) MathF.Round((viewportSize.Width - (boundsSize.Width * ratio)) / 2.0f) + newX;
            var posY = (int) MathF.Round((viewportSize.Height - (boundsSize.Height * ratio)) / 2.0f) + newY;

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

        public bool Contains(in Point2D point) => Contains(point.X, point.Y);
        public bool Contains(in Vector2 point) => Contains(point.X, point.Y);

        public bool Contains(float x, float y)
        {
            return x >= X
                && x <= Right
                && y >= Y
                && y <= Bottom;
        }


        public bool Contains(in RectangleF rect)
        {
            return rect.Left >= Left
                && rect.Right <= Right
                && rect.Top >= Top
                && rect.Bottom <= Bottom;
        }

        public bool Intersects(in RectangleF rect)
        {
            return rect.Left <= Right
                && rect.Right >= Left
                && rect.Top <= Bottom
                && rect.Bottom >= Top;
        }

        public bool Intersects(in Vector2 center, float radius)
        {
            var halfWidth = Width / 2.0f;
            var halfHeight = Height / 2.0f;

            var circleDistanceX = MathF.Abs(center.X - (X + halfWidth));
            var circleDistanceY = MathF.Abs(center.Y - (Y + halfHeight));

            if (circleDistanceX > halfWidth + radius ||
                circleDistanceY > halfHeight + radius)
            {
                return false;
            }

            if (circleDistanceX <= halfWidth ||
                circleDistanceY <= halfHeight)
            {
                return true;
            }

            var cornerDistanceSquared = MathF.Pow(circleDistanceX - halfWidth, 2) +
                                        MathF.Pow(circleDistanceY - halfHeight, 2);

            return cornerDistanceSquared <= MathF.Pow(radius, 2);
        }

        public bool Intersects(in TransformedRectangle rect) => rect.Intersects(TransformedRectangle.FromRectangle(this));

        // TODO: It might make sense to micro-optimize this, as it's a very common operation.
        public ContainmentType Intersect(in RectangleF rect)
        {
            if (Contains(rect))
            {
                return ContainmentType.Contains;
            }

            if (Intersects(rect))
            {
                return ContainmentType.Intersects;
            }

            return ContainmentType.Disjoint;
        }

        public RectangleF WithY(float y) => new RectangleF(X, y, Width, Height);

        public RectangleF WithWidth(float width) => new RectangleF(X, Y, width, Height);

        public bool Equals(in RectangleF other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y) && Width.Equals(other.Width) && Height.Equals(other.Height);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            return obj is RectangleF other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X.GetHashCode(), Y.GetHashCode(), Width.GetHashCode(), Height.GetHashCode());
        }

        public static RectangleF Scale(in RectangleF rect, float factor)
        {
            var newWidth = rect.Width * factor;
            var newHeight = rect.Height * factor;
            var deltaWidth = rect.Width - newWidth;
            var deltaHeight = rect.Height - newHeight;
            return new RectangleF(rect.X + deltaWidth / 2, rect.Y + deltaHeight / 2, newWidth, newHeight);
        }
    }
}
