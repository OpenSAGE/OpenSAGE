using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Graphics.Cameras;

namespace OpenSage.Mathematics
{
    public struct BoundingBox
    {
        public Vector3 Min;
        public Vector3 Max;

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
            var result = new BoundingBox
            {
                Min = Vector3.Min(original.Min, additional.Min),
                Max = Vector3.Max(original.Max, additional.Max)
            };
            return result;
        }

        public static BoundingBox CreateFromSphere(in BoundingSphere sphere)
        {
            var corner = new Vector3(sphere.Radius);
            return new BoundingBox
            {
                Min = sphere.Center - corner,
                Max = sphere.Center + corner
            };
        }

        public Vector3 GetCenter() => (Min + Max) / 2;

        public PlaneIntersectionType Intersects(ref Plane plane)
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

        // Based on http://dev.theomader.com/transform-bounding-boxes/
        public BoundingBox Transform(in Matrix4x4 matrix)
        {
            var right = matrix.Right();
            var xa = right * Min.X;
            var xb = right * Max.X;

            var up = matrix.Up();
            var ya = up * Min.Y;
            var yb = up * Max.Y;

            var backward = matrix.Backward();
            var za = backward * Min.Z;
            var zb = backward * Max.Z;

            return new BoundingBox(
                Vector3.Min(xa, xb) + Vector3.Min(ya, yb) + Vector3.Min(za, zb) + matrix.Translation,
                Vector3.Max(xa, xb) + Vector3.Max(ya, yb) + Vector3.Max(za, zb) + matrix.Translation
            );
        }

        public override string ToString()
        {
            return $"{nameof(Min)}: {Min}, {nameof(Max)}: {Max}";
        }

        public Rectangle ToScreenRectangle(CameraComponent camera)
        {
            var corners = new[]
            {
                // Bottom plane
                Min,
                Min.WithX(Max.X),
                Min.WithY(Max.Y),
                Max.WithZ(Min.Z),
                // Top plane
                Max,
                Max.WithX(Min.X),
                Max.WithY(Min.Y),
                Min.WithZ(Max.Z)
            };

            var topLeft = new Vector3(float.MaxValue);
            var bottomRight = new Vector3(float.MinValue);

            for (var i = 0; i < corners.Length; i++)
            {
                var screenPos = camera.WorldToScreenPoint(corners[i]);
                topLeft = Vector3.Min(topLeft, screenPos);
                bottomRight = Vector3.Max(bottomRight, screenPos);
            }

            var size = bottomRight - topLeft;

            return new Rectangle((int) topLeft.X, (int) topLeft.Y, (int) size.X, (int) size.Y);
        }
    }
}
