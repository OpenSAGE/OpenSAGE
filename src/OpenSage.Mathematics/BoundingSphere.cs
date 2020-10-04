﻿using System;
using System.Numerics;

namespace OpenSage.Mathematics
{
    public readonly struct BoundingSphere : BoundingVolume
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

        public static BoundingSphere CreateFromBoundingBox(BoundingBox box)
        {
            // Find the center of the box.
            var center = new Vector3((box.Min.X + box.Max.X) / 2.0f,
                                         (box.Min.Y + box.Max.Y) / 2.0f,
                                         (box.Min.Z + box.Max.Z) / 2.0f);

            // Find the distance between the center and one of the corners of the box.
            var radius = Vector3.Distance(center, box.Max);

            return new BoundingSphere(center, radius);
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
            else if (distance < -Radius)
            {
                return PlaneIntersectionType.Back;
            }
            else
            {
                return PlaneIntersectionType.Intersecting;
            }
        }
    }
}
