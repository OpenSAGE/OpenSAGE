using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace OpenSage.Mathematics
{
    public readonly struct BoundingBox : IBoundingVolume
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

        public static BoundingBox CreateFromPoints(params Vector3[] points) => CreateFromPoints(points.AsEnumerable());

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

        public bool Contains(in Vector3 position)
        {
            return position.X >= Min.X &&
                   position.Y >= Min.Y &&
                   position.Z >= Min.Z &&
                   position.X <= Max.X &&
                   position.Y <= Max.Y &&
                   position.Z <= Max.Z;
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

        public bool Intersects(RectangleF bounds)
        {
            var position = Min.Vector2XY();
            var maxPosition = Max.Vector2XY();
            var width = maxPosition.X - position.X;
            var height = maxPosition.Y - position.Y;
            var rect = new RectangleF(position, width, height);
            return rect.Intersects(bounds);
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

            var translation = matrix.Translation;

            return new BoundingBox(
                Vector3.Min(xa, xb) + Vector3.Min(ya, yb) + Vector3.Min(za, zb) + translation,
                Vector3.Max(xa, xb) + Vector3.Max(ya, yb) + Vector3.Max(za, zb) + translation
            );
        }

        public override string ToString()
        {
            return $"{nameof(Min)}: {Min}, {nameof(Max)}: {Max}";
        }
    }
}
