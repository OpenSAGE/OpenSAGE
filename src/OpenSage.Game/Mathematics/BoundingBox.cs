﻿using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Graphics.Cameras;

namespace OpenSage.Mathematics
{
    public readonly struct BoundingBox
    {
        public readonly Vector3 Min;
        public readonly Vector3 Max;

        public BoundingBox(in Vector3 min, in Vector3 max)
        {
            Min = min;
            Max = max;
        }

        private static readonly Vector3 MaxVector3 = new Vector3(float.MaxValue);
        private static readonly Vector3 MinVector3 = new Vector3(float.MinValue);

        public static BoundingBox CreateFromPoints(IEnumerable<Vector3> points)
        {
            var empty = true;
            var minVec = MaxVector3;
            var maxVec = MinVector3;

            foreach (var ptVector in points)
            {
                minVec = Vector3.Min(minVec, ptVector);
                maxVec = Vector3.Max(maxVec, ptVector);
                empty = false;
            }

            if (empty)
            {
                throw new ArgumentException();
            }

            return new BoundingBox(minVec, maxVec);
        }

        public static BoundingBox CreateMerged(in BoundingBox original, in BoundingBox additional)
        {
            return new BoundingBox(
                Vector3.Min(original.Min, additional.Min),
                Vector3.Max(original.Max, additional.Max));
        }

        public static BoundingBox CreateFromSphere(in BoundingSphere sphere)
        {
            var corner = new Vector3(sphere.Radius);
            return new BoundingBox(
                sphere.Center - corner,
                sphere.Center + corner);
        }

        public Vector3 GetCenter()
        {
            return (Min + Max) / 2;
        }

        public PlaneIntersectionType Intersects(in Plane plane)
        {
            // See http://zach.in.tu-clausthal.de/teaching/cg_literatur/lighthouse3d_view_frustum_culling/index.html

            Vector3 positiveVertex;
            Vector3 negativeVertex;

            if (plane.Normal.X >= 0)
            {
                positiveVertex.X = Max.X;
                negativeVertex.X = Min.X;
            }
            else
            {
                positiveVertex.X = Min.X;
                negativeVertex.X = Max.X;
            }

            if (plane.Normal.Y >= 0)
            {
                positiveVertex.Y = Max.Y;
                negativeVertex.Y = Min.Y;
            }
            else
            {
                positiveVertex.Y = Min.Y;
                negativeVertex.Y = Max.Y;
            }

            if (plane.Normal.Z >= 0)
            {
                positiveVertex.Z = Max.Z;
                negativeVertex.Z = Min.Z;
            }
            else
            {
                positiveVertex.Z = Min.Z;
                negativeVertex.Z = Max.Z;
            }

            // Inline Vector3.Dot(plane.Normal, negativeVertex) + plane.D;
            var planeNormal = plane.Normal;
            var distance = planeNormal.X * negativeVertex.X + planeNormal.Y * negativeVertex.Y + planeNormal.Z * negativeVertex.Z + plane.D;
            if (distance > 0)
            {
                return PlaneIntersectionType.Front;
            }

            // Inline Vector3.Dot(plane.Normal, positiveVertex) + plane.D;
            distance = planeNormal.X * positiveVertex.X + planeNormal.Y * positiveVertex.Y + planeNormal.Z * positiveVertex.Z + plane.D;
            if (distance < 0)
            {
                return PlaneIntersectionType.Back;
            }

            return PlaneIntersectionType.Intersecting;
        }

        // Based on MonoGame
        // https://github.com/MonoGame/MonoGame/blob/faea8a6a89504673e2bb7e435f0da8cc513d8c30/MonoGame.Framework/BoundingBox.cs#L362
        public bool Intersects(in BoundingBox box)
        {
            if ((Max.X >= box.Min.X) && (Min.X <= box.Max.X))
            {
                if ((Max.Y < box.Min.Y) || (Min.Y > box.Max.Y))
                {
                    return false;
                }

                return (Max.Z >= box.Min.Z) && (Min.Z <= box.Max.Z);
            }

            return false;
        }

        // Based on MonoGame:
        // https://github.com/MonoGame/MonoGame/blob/faea8a6a89504673e2bb7e435f0da8cc513d8c30/MonoGame.Framework/BoundingBox.cs#L385
        public bool Intersects(in BoundingSphere sphere)
        {
            if (sphere.Center.X - Min.X > sphere.Radius
                && sphere.Center.Y - Min.Y > sphere.Radius
                && sphere.Center.Z - Min.Z > sphere.Radius
                && Max.X - sphere.Center.X > sphere.Radius
                && Max.Y - sphere.Center.Y > sphere.Radius
                && Max.Z - sphere.Center.Z > sphere.Radius)
            {
                return true;
            }

            float dmin = 0;

            if (sphere.Center.X - Min.X <= sphere.Radius)
            {
                dmin += (sphere.Center.X - Min.X) * (sphere.Center.X - Min.X);
            }
            else if (Max.X - sphere.Center.X <= sphere.Radius)
            {
                dmin += (sphere.Center.X - Max.X) * (sphere.Center.X - Max.X);
            }

            if (sphere.Center.Y - Min.Y <= sphere.Radius)
            {
                dmin += (sphere.Center.Y - Min.Y) * (sphere.Center.Y - Min.Y);
            }
            else if (Max.Y - sphere.Center.Y <= sphere.Radius)
            {
                dmin += (sphere.Center.Y - Max.Y) * (sphere.Center.Y - Max.Y);
            }

            if (sphere.Center.Z - Min.Z <= sphere.Radius)
            {
                dmin += (sphere.Center.Z - Min.Z) * (sphere.Center.Z - Min.Z);
            }
            else if (Max.Z - sphere.Center.Z <= sphere.Radius)
            {
                dmin += (sphere.Center.Z - Max.Z) * (sphere.Center.Z - Max.Z);
            }

            if (dmin <= sphere.Radius * sphere.Radius)
            {
                return true;
            }

            return false;
        }

        // Based on http://dev.theomader.com/transform-bounding-boxes/
        public static BoundingBox Transform(in BoundingBox box, in Matrix4x4 matrix)
        {
            var right = matrix.Right();
            var xa = right * box.Min.X;
            var xb = right * box.Max.X;

            var up = matrix.Up();
            var ya = up * box.Min.Y;
            var yb = up * box.Max.Y;

            var backward = matrix.Backward();
            var za = backward * box.Min.Z;
            var zb = backward * box.Max.Z;

            var translation = Matrix4x4Utility.GetTranslation(matrix);

            return new BoundingBox(
                Vector3.Min(xa, xb) + Vector3.Min(ya, yb) + Vector3.Min(za, zb) + translation,
                Vector3.Max(xa, xb) + Vector3.Max(ya, yb) + Vector3.Max(za, zb) + translation
            );
        }

        public override string ToString()
        {
            return $"{nameof(Min)}: {Min}, {nameof(Max)}: {Max}";
        }

        /// <summary>
        /// Converts the bounding box from world space into a screen space bounding rectangle.
        /// </summary>
        public Rectangle GetBoundingRectangle(Camera camera)
        {
            var topLeft = new Vector3(float.MaxValue);
            var bottomRight = new Vector3(float.MinValue);

            Span<Vector3> vertices = stackalloc Vector3[8];

            // Bottom plane
            vertices[0] = Min;
            vertices[1] = Min.WithX(Max.X);
            vertices[2] = Min.WithY(Max.Y);
            vertices[3] = Max.WithZ(Min.Z);

            // Top plane
            vertices[4] = Max;
            vertices[5] = Max.WithX(Min.X);
            vertices[6] = Max.WithY(Min.Y);
            vertices[7] = Min.WithZ(Max.Z);

            for (var i = 0; i < 8; i++)
            {
                var screenPos = camera.WorldToScreenPoint(vertices[i]);
                topLeft = Vector3.Min(topLeft, screenPos);
                bottomRight = Vector3.Max(bottomRight, screenPos);
            }

            var size = bottomRight - topLeft;
            return new Rectangle((int) topLeft.X, (int) topLeft.Y, (int) size.X, (int) size.Y);
        }
    }
}
