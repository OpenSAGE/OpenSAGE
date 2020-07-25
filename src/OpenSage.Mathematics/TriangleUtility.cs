using System.Numerics;

namespace OpenSage.Mathematics
{
    public static class TriangleUtility
    {
        public static bool IsPointInside(in Vector2 v1, in Vector2 v2, in Vector2 v3, in Point2D point)
        {
            return IsPointInside(v1, v2, v3, point.X, point.Y);
        }

        public static bool IsPointInside(in Vector2 v1, in Vector2 v2, in Vector2 v3, in Vector2 point)
        {
            return IsPointInside(v1, v2, v3, point.X, point.Y);
        }

        public static bool IsPointInside(in Vector2 v1, in Vector2 v2, in Vector2 v3, in float pointX, in float pointY)
        {
            var denominator = ((v2.Y - v3.Y) * (v1.X - v3.X) + (v3.X - v2.X) * (v1.Y - v3.Y));
            var a = ((v2.Y - v3.Y) * (pointX - v3.X) + (v3.X - v2.X) * (pointY - v3.Y)) / denominator;
            var b = ((v3.Y - v1.Y) * (pointX - v3.X) + (v1.X - v3.X) * (pointY - v3.Y)) / denominator;
            var c = 1 - a - b;

            return 0 <= a && a <= 1 && 0 <= b && b <= 1 && 0 <= c && c <= 1;
        }
    }
}
