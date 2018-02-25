using System;
using System.Numerics;

namespace OpenSage.Mathematics
{
    public struct Point2D
    {
        public static readonly Point2D Zero = new Point2D(0, 0);

        public int X;
        public int Y;

        public Point2D(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return $"<{X}, {Y}>";
        }

        public static Point2D Transform(in Point2D point, in Matrix3x2 matrix)
        {
            var result = Vector2.Transform(new Vector2(point.X, point.Y), matrix);
            return new Point2D(
                (int) Math.Round(result.X),
                (int) Math.Round(result.Y));
        }
    }
}
