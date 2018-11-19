using System;
using System.Numerics;

namespace OpenSage.Mathematics
{
    public struct Point2Df
    {
        public static readonly Point2Df Zero = new Point2Df(0, 0);

        public float X;
        public float Y;

        public Point2Df(float x, float y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return $"<{X}, {Y}>";
        }

        public static Point2Df operator+(Point2Df a, Point2Df b)
        {
            return new Point2Df(a.X + b.X, a.Y + b.Y);
        }

        public static Point2Df operator-(Point2Df a, Point2Df b)
        {
            return new Point2Df(a.X - b.X, a.Y - b.Y);
        }

        public static Point2Df Min(in Point2Df a, in Point2Df b)
        {
            return new Point2Df(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
        }

        public static Point2Df Max(in Point2Df a, in Point2Df b)
        {
            return new Point2Df(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
        }
    }
}
