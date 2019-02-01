using System.Numerics;

namespace OpenSage.Mathematics
{
    public static class Vector2Utility
    {
        // https://stackoverflow.com/a/2259502/486974
        public static Vector2 RotateAroundPoint(Vector2 axis, Vector2 point, float angle)
        {
            var sin = MathUtility.Sin(angle);
            var cos = MathUtility.Cos(angle);

            point -= axis;
            var rotated = new Vector2(point.X * cos - point.Y * sin, point.X * sin + point.Y  * cos);
            return rotated + axis;
        }

        public static float CrossProductLength(Vector2 first, Vector2 second)
        {
            return (first.X * second.Y) - (first.Y * second.X);
        }

        public static bool IsCongruentTo(this Vector2 point, in Vector2 otherPoint)
        {
            return Vector2.Distance(point, otherPoint) < Constants.EPSILON;

        }
    }
}
