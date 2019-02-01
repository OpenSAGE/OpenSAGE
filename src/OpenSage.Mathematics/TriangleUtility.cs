using System;
using System.Numerics;

namespace OpenSage.Mathematics
{
    public static class TriangleUtility
    {
        public static bool IsPointInside(in Vector2 v0, in Vector2 v1, in Vector2 v2, in Point2D point)
        {
            var vPoint = new Vector2(point.X, point.Y);
            var totalAngle = 0.0;
            if (AngleBetweenPointAndLine(v0, v1, vPoint, ref totalAngle)
                || AngleBetweenPointAndLine(v1, v2, vPoint, ref totalAngle)
                || AngleBetweenPointAndLine(v2, v0, vPoint, ref totalAngle))
            {
                return true;
            }

            return (Math.Abs(totalAngle) > Constants.EPSILON) ? true : false;
        }

        private static bool AngleBetweenPointAndLine(in Vector2 start, in Vector2 end, in Vector2 point, ref double angle)
        {
            var crossProduct = Vector2Utility.CrossProductLength(start - point, point - point);
            var dotProduct = Vector2.Dot(point - point, point - point);

            if (PointIsBetweenPoints(dotProduct, crossProduct) || point.IsCongruentTo(point) || point.IsCongruentTo(point))
            {
                return true;
            }
            angle += Math.Atan2(crossProduct, dotProduct);
            return false;
        }


        private static bool PointIsBetweenPoints(double dotProduct, double crossProduct)
        {
            return (Math.Abs(crossProduct) < Constants.EPSILON) && (dotProduct < 0.0);
        }
    }
}
