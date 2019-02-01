using System;
using System.Numerics;

namespace OpenSage.Mathematics
{
    public static class TriangleUtility
    {
        public static bool IsPointInside(in Vector2 v1, in Vector2 v2, in Vector2 v3, in Point2D point)
        {
            var denominator = ((v2.Y - v3.Y)*(v1.X - v3.X) + (v3.X - v2.X)* (v1.Y - v3.Y));
            var a = ((v2.Y - v3.Y) * (point.X - v3.X) + (v3.X - v2.X) * (point.Y - v3.Y)) / denominator;
            var b = ((v3.Y - v1.Y) * (point.X - v3.X) + (v1.X - v3.X) * (point.Y - v3.Y)) / denominator;
            var c = 1 - a - b;
 
            return 0 <= a && a <= 1 && 0 <= b && b <= 1 && 0 <= c && c <= 1;
        }
    }
}
