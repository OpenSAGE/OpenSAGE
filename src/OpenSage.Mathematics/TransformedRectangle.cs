using System;
using System.Numerics;

namespace OpenSage.Mathematics
{
    public readonly struct TransformedRectangle
    {
        // TODO: There must be a more memory-efficient representation.

        public readonly Vector2 UpperLeft;
        public readonly Vector2 UpperRight;
        public readonly Vector2 LowerLeft;
        public readonly Vector2 LowerRight;

        public Vector2 Center => (UpperLeft + LowerRight) / 2; 

        public TransformedRectangle(Vector2 upperLeft, Vector2 upperRight, Vector2 lowerLeft, Vector2 lowerRight)
        {
            UpperLeft = upperLeft;
            UpperRight = upperRight;
            LowerLeft = lowerLeft;
            LowerRight = lowerRight;
        }

        // Based on
        // https://www.gamedev.net/articles/programming/general-and-gameplay-programming/2d-rotated-rectangle-collision-r2604
        // https://www.habrador.com/tutorials/math/7-rectangle-rectangle-intersection/
        public bool Intersects(in TransformedRectangle other)
        {
            var axis1 = UpperRight - UpperLeft;
            var axis2 = UpperRight - LowerRight;

            var axis3 = other.UpperLeft - other.LowerLeft;
            var axis4 = other.UpperLeft - other.UpperRight;

            (float Min, float Max) ComputeMinMax(Vector2 axis, in TransformedRectangle rect)
            {
                var proj1 = Vector2.Dot(axis, rect.UpperLeft);
                var proj2 = Vector2.Dot(axis, rect.UpperRight);
                var proj3 = Vector2.Dot(axis, rect.LowerLeft);
                var proj4 = Vector2.Dot(axis, rect.LowerRight);

                var min = Math.Min(proj1, Math.Min(proj2, Math.Min(proj3, proj4)));
                var max = Math.Max(proj1, Math.Max(proj2, Math.Max(proj3, proj4)));

                return (min, max);
            }

            bool ProjectionsIntersect(Vector2 axis, in TransformedRectangle a, in TransformedRectangle b)
            {
                var (min1, max1) = ComputeMinMax(axis, a);
                var (min2, max2) = ComputeMinMax(axis, b);
                return min1 <= max2 && min2 <= max1;
            }

            if (!ProjectionsIntersect(axis1, this, other))
            {
                return false;
            }

            if (!ProjectionsIntersect(axis2, this, other))
            {
                return false;
            }

            if (!ProjectionsIntersect(axis3, this, other))
            {
                return false;
            }

            if (!ProjectionsIntersect(axis4, this, other))
            {
                return false;
            }

            return true;
        }

        public bool Intersects(in Vector2 center, float radius)
        {
            var width = (LowerRight - LowerLeft).Length();
            var height = (UpperLeft - LowerLeft).Length();
            var rectF = new RectangleF(Vector2.Zero, width, height);

            var rectAngle = Vector2Utility.Angle(LowerLeft, LowerRight);
            var newCenter = center - LowerLeft;
            newCenter = newCenter.RotateAroundPoint(LowerLeft, -rectAngle);

            return rectF.Intersects(newCenter, radius);
        }

        public bool Contains(Vector2 point)
        {
            return TriangleUtility.IsPointInside(LowerLeft, UpperLeft, UpperRight, point)
                || TriangleUtility.IsPointInside(LowerLeft, UpperRight, LowerRight, point);
        }

        public static TransformedRectangle FromRectangle(in RectangleF rect, float angle = 0)
        {
            var upperLeft = new Vector2(rect.X, rect.Y);
            var upperRight = upperLeft + new Vector2(rect.Width, 0);
            var lowerLeft = upperLeft + new Vector2(0, rect.Height);
            var lowerRight = upperLeft + new Vector2(rect.Width, rect.Height);

            var center = (upperLeft + lowerRight) / 2;

            upperLeft = upperLeft.RotateAroundPoint(center, angle);
            upperRight = upperRight.RotateAroundPoint(center, angle);
            lowerLeft = lowerLeft.RotateAroundPoint(center, angle);
            lowerRight = lowerRight.RotateAroundPoint(center, angle);

            return new TransformedRectangle(upperLeft, upperRight, lowerLeft, lowerRight);
        }
    }
}
