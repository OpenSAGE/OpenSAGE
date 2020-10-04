using System;
using System.Numerics;

namespace OpenSage.Mathematics
{
    public static class Vector2Utility
    {
        // https://stackoverflow.com/a/2259502/486974
        public static Vector2 RotateAroundPoint(this Vector2 point, Vector2 center, float angle)
        {
            var sin = MathF.Sin(angle);
            var cos = MathF.Cos(angle);

            point -= center;
            var rotated = new Vector2(point.X * cos - point.Y * sin, point.X * sin + point.Y  * cos);
            return rotated + center;
        }

        public static float Angle(this Vector2 point, Vector2 point2)
        {
            return MathF.Atan2(point.Y - point2.Y, point.X - point2.X);
        }
    }
}
