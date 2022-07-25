using System;
using System.Numerics;

namespace OpenSage.Mathematics
{
    public class Point2D
    {
        public static Point2D Zero = new Point2D(0, 0);

        public int X { get; set; }
        public int Y { get; set; }

        public Point2D()
        {
        }

        public Point2D(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public override string ToString()
        {
            return $"<{X}, {Y}>";
        }

        public static Point2D Transform(in Point2D point, in Matrix3x2 matrix)
        {
            var result = Vector2.Transform(new Vector2(point.X, point.Y), matrix);
            return new Point2D(
                (int) MathF.Round(result.X),
                (int) MathF.Round(result.Y));
        }

        public Size ToSize()
        {
            return new Size(X, Y);
        }

        public Vector2 ToVector2()
        {
            return new Vector2(X, Y);
        }

        public static Point2D operator+(Point2D a, Point2D b)
        {
            return new Point2D(a.X + b.X, a.Y + b.Y);
        }

        public static Point2D operator-(Point2D a, Point2D b)
        {
            return new Point2D(a.X - b.X, a.Y - b.Y);
        }

        public static Point2D Min(in Point2D a, in Point2D b)
        {
            return new Point2D(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
        }

        public static Point2D Max(in Point2D a, in Point2D b)
        {
            return new Point2D(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
        }
    }
}
