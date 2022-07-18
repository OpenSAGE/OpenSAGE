using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace OpenSage.Mathematics
{
    public readonly struct AxisAlignedBoundingBox : IBoundingVolume
    {
        public readonly Vector3 Min;
        public readonly Vector3 Max;

        public AxisAlignedBoundingBox(in Vector3 min, in Vector3 max)
        {
            Min = min;
            Max = max;
        }

        public AxisAlignedBoundingBox(in BoundingBox box)
        {
            Min = MaxVector3;
            Max = MinVector3;

            foreach (var vertex in box.Vertices)
            {
                if (vertex.X < Min.X) Min.X = vertex.X;
                if (vertex.Y < Min.Y) Min.Y = vertex.Y;
                if (vertex.Z < Min.Z) Min.Z = vertex.Z;

                if (vertex.X > Max.X) Max.X = vertex.X;
                if (vertex.Y > Max.Y) Max.Y = vertex.Y;
                if (vertex.Z > Max.Z) Max.Z = vertex.Z;
            }
        }

        private static readonly Vector3 MaxVector3 = new Vector3(float.MaxValue);
        private static readonly Vector3 MinVector3 = new Vector3(float.MinValue);

        public static AxisAlignedBoundingBox CreateFromPoints(params Vector3[] points) => CreateFromPoints(points.AsEnumerable());

        public static AxisAlignedBoundingBox CreateFromPoints(IEnumerable<Vector3> points)
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

            return new AxisAlignedBoundingBox(minVec, maxVec);
        }

        public static AxisAlignedBoundingBox CreateMerged(in AxisAlignedBoundingBox original, in AxisAlignedBoundingBox additional)
        {
            return new AxisAlignedBoundingBox(
                Vector3.Min(original.Min, additional.Min),
                Vector3.Max(original.Max, additional.Max));
        }

        public static AxisAlignedBoundingBox CreateFromSphere(in BoundingSphere sphere)
        {
            var corner = new Vector3(sphere.Radius);
            return new AxisAlignedBoundingBox(
                sphere.Center - corner,
                sphere.Center + corner);
        }

        public Vector3 GetCenter() => (Min + Max) / 2;

        public bool Contains(in Vector3 position)
        {
            const float epsilon = 0.01f;
            return position.X >= Min.X - epsilon &&
                   position.Y >= Min.Y - epsilon &&
                   position.Z >= Min.Z - epsilon &&
                   position.X <= Max.X + epsilon &&
                   position.Y <= Max.Y + epsilon &&
                   position.Z <= Max.Z + epsilon;
        }

        public bool Intersects(in BoundingSphere sphere) => sphere.Intersects(this);

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
        public static AxisAlignedBoundingBox Transform(in AxisAlignedBoundingBox box, in Matrix4x4 matrix)
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

            return new AxisAlignedBoundingBox(
                Vector3.Min(xa, xb) + Vector3.Min(ya, yb) + Vector3.Min(za, zb) + translation,
                Vector3.Max(xa, xb) + Vector3.Max(ya, yb) + Vector3.Max(za, zb) + translation
            );
        }

        public bool Intersects(in AxisAlignedBoundingBox other)
        {
            return Max.X >= other.Min.X && Min.X <= other.Max.X
                && Max.Y >= other.Min.Y && Min.Y <= other.Max.Y
                && Max.Z >= other.Min.Z && Min.Z <= other.Max.Z;
        }

        public override string ToString()
        {
            return $"{nameof(Min)}: {Min}, {nameof(Max)}: {Max}";
        }
    }
}
