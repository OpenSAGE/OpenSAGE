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
            var distance = Vector3.Dot(plane.Normal, Center) + plane.D;
            if (distance > Radius)
            {
                return PlaneIntersectionType.Front;
            }
            return distance < -Radius ? PlaneIntersectionType.Back : PlaneIntersectionType.Intersecting;
        }

        public bool Intersects(RectangleF bounds) => bounds.Intersects(Center.Vector2XY(), Radius);
        public bool Intersects(TransformedRectangle bounds) => bounds.Intersects(Center.Vector2XY(), Radius);

        public bool Intersects(in AxisAlignedBoundingBox box)
        {
            // See https://developer.mozilla.org/en-US/docs/Games/Techniques/3D_collision_detection
            var x = MathF.Max(box.Min.X, MathF.Min(Center.X, box.Max.X));
            var y = MathF.Max(box.Min.Y, MathF.Min(Center.Y, box.Max.Y));
            var z = MathF.Max(box.Min.Z, MathF.Min(Center.Z, box.Max.Z));

            return Contains(x, y, z);
        }

        public bool Intersects(in BoundingBox box) => box.Intersects(this);

        public bool Contains(float x, float y, float z)
        {
            var distance = MathF.Sqrt((x - Center.X) * (x - Center.X) +
                                     (y - Center.Y) * (y - Center.Y) +
                                     (z - Center.Z) * (z - Center.Z));

            return distance <= Radius;
        }

        public bool Contains(Vector2 point)
        {
            var distance = (Center.Vector2XY() - point).Length();
            return distance <= Radius;
        }
    }
}
