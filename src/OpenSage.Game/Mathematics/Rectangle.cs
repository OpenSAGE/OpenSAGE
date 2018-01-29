using System;
using System.Numerics;

namespace OpenSage.Mathematics
{
    public readonly struct Rectangle
    {
        public readonly int X;
        public readonly int Y;
        public readonly int Width;
        public readonly int Height;

        public int Left => X;
        public int Right => X + Width;
        public int Top => Y;
        public int Bottom => Y + Height;

        public Rectangle(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public Rectangle(in Point2D location, in Size size)
        {
            X = location.X;
            Y = location.Y;
            Width = size.Width;
            Height = size.Height;
        }

        public bool Intersects(in Rectangle value)
        {
            return value.Left < Right &&
                Left < value.Right &&
                value.Top < Bottom &&
                Top < value.Bottom;
        }

        public static Rectangle Intersect(in Rectangle value1, in Rectangle value2)
        {
            if (value1.Intersects(value2))
            {
                int right_side = Math.Min(value1.X + value1.Width, value2.X + value2.Width);
                int left_side = Math.Max(value1.X, value2.X);
                int top_side = Math.Max(value1.Y, value2.Y);
                int bottom_side = Math.Min(value1.Y + value1.Height, value2.Y + value2.Height);
                return new Rectangle(left_side, top_side, right_side - left_side, bottom_side - top_side);
            }
            else
            {
                return new Rectangle(0, 0, 0, 0);
            }
        }

        public bool Contains(in Vector2 point)
        {
            return point.X >= X
                && point.X <= Right
                && point.Y >= Y
                && point.Y <= Bottom;
        }

        public RectangleF ToRectangleF()
        {
            return new RectangleF(X, Y, Width, Height);
        }
    }
}
