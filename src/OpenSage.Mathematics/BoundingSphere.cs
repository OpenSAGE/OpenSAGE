using System;
using System.Numerics;

namespace OpenSage.Mathematics
{
    public readonly struct BoundingSphere : IBoundingVolume
    {
        public readonly Vector3 Center;

        public readonly float Radius;

        public BoundingSphere(in Vector3 center, float radius)
        {
            Center = center;
            Radius = radius;
        }

        public static BoundingSphere CreateMerged(in BoundingSphere original, in BoundingSphere additional)
        {
            var ocenterToaCenter = Vector3.Subtract(additional.Center, original.Center);
            var distance = ocenterToaCenter.Length();
            if (distance <= original.Radius + additional.Radius)
            {
                if (distance <= original.Radius - additional.Radius)
                {
                    return original;
                }
                if (distance <= additional.Radius - original.Radius)
                {
                    return additional;
                }
            }
            var leftRadius = Math.Max(original.Radius - distance, additional.Radius);
            var rightRadius = Math.Max(original.Radius + distance, additional.Radius);
            ocenterToaCenter += ((leftRadius - rightRadius) / (2 * ocenterToaCenter.Length()) * ocenterToaCenter);

            return new BoundingSphere(
                original.Center + ocenterToaCenter,
                (leftRadius + rightRadius) / 2);
        }

        public static BoundingSphere Transform(in BoundingSphere sphere, in Matrix4x4 matrix)
        {
            return new BoundingSphere(
                Vector3.Transform(sphere.Center, matrix),
                sphere.Radius * (MathF.Sqrt(
                    Math.Max(
                        ((matrix.M11 * matrix.M11) + (matrix.M12 * matrix.M12)) + (matrix.M13 * matrix.M13),
                        Math.Max(
                            ((matrix.M21 * matrix.M21) + (matrix.M22 * matrix.M22)) + (matrix.M23 * matrix.M23),
                            ((matrix.M31 * matrix.M31) + (matrix.M32 * matrix.M32)) + (matrix.M33 * matrix.M33))))));
        }

        public PlaneIntersectionType Intersects(in Plane plane)
        {
            var distance = Vector3.Dot(plane.Normal, Center);
            distance += plane.D;
            if (distance > Radius)
            {
                return PlaneIntersectionType.Front;
            }
            return distance < -Radius ? PlaneIntersectionType.Back : PlaneIntersectionType.Intersecting;
        }

        public bool Intersects(RectangleF bounds)
        {
            var halfWidth = bounds.Width / 2.0f;
            var halfHeight = bounds.Height / 2.0f;

            var circleDistanceX = MathF.Abs(Center.X - (bounds.X + halfWidth));
            var circleDistanceY = MathF.Abs(Center.Y - (bounds.Y + halfHeight));

            if (circleDistanceX > halfWidth + Radius ||
                circleDistanceY > halfHeight + Radius)
            {
                return false;
            }

            if (circleDistanceX <= halfWidth ||
                circleDistanceY <= halfHeight)
            {
                return true;
            }

            var cornerDistanceSquared = MathF.Pow((circleDistanceX - halfWidth), 2) +
                                        MathF.Pow((circleDistanceY - halfHeight), 2);

            return cornerDistanceSquared <= MathF.Pow(Radius, 2);
        }
    }
}
