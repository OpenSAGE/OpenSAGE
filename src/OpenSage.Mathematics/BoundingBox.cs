using System;
using System.Numerics;

namespace OpenSage.Mathematics
{
    public readonly struct BoundingBox : IBoundingVolume
    {
        private readonly Vector3 _center;
        private readonly Vector3 _size;
        private readonly Vector3 _right;
        private readonly Vector3 _up;
        private readonly Vector3 _forward;
        private readonly Matrix4x4 _matrix;
        private readonly AxisAlignedBoundingBox _aaBox;

        public readonly Vector3[] Vertices;

        public BoundingBox(in RectangleF rect, float height, float z = 0.0f)
        {
            _matrix = Matrix4x4.Identity;
            _size = new Vector3(rect.Width, rect.Height, height);
            _center = new Vector3(rect.X + rect.Width / 2.0f, rect.Y + rect.Height / 2.0f, z + (height / 2.0f));
            Vertices = new[]
            {
                new Vector3(rect.X, rect.Y, z),
                new Vector3(rect.X + rect.Width, rect.Y, z),
                new Vector3(rect.X, rect.Y + rect.Height, z),
                new Vector3(rect.X + rect.Width, rect.Y + rect.Height, z),

                new Vector3(rect.X, rect.Y, z + height),
                new Vector3(rect.X + rect.Width, rect.Y, z + height),
                new Vector3(rect.X, rect.Y + rect.Height, z + height),
                new Vector3(rect.X + rect.Width, rect.Y + rect.Height, z + height),
            };

            _aaBox = new AxisAlignedBoundingBox(Vertices[0], Vertices[7]);
            _right = Vector3.UnitX;
            _up = Vector3.UnitZ;
            _forward = Vector3.UnitY;
        }

        public BoundingBox(in AxisAlignedBoundingBox box, Matrix4x4 matrix) :
            this(new RectangleF(box.Min.X, box.Min.Y, box.Max.X - box.Min.X, box.Max.Y - box.Min.Y), box.Max.Z - box.Min.Z, box.Min.Z)
        {
            _matrix = matrix;
            if (Matrix4x4.Decompose(matrix, out var scale, out var rotation, out _))
            {
                _size *= scale;
                _right = Vector3.Transform(_right, rotation);
                _up = Vector3.Transform(_up, rotation);
                _forward = Vector3.Transform(_forward, rotation);
            }
            _center = Vector3.Transform(_center, matrix);
            for (var i = 0; i < Vertices.Length; i++)
            {
                Vertices[i] = Vector3.Transform(Vertices[i], matrix);
            }
        }

        public bool Intersects(RectangleF bounds) => throw new NotImplementedException();

        public bool Intersects(in BoundingBox box)
        {
            // Based on https://gamedev.stackexchange.com/questions/44500/how-many-and-which-axes-to-use-for-3d-obb-collision-with-sat
            if (Separated(Vertices, box.Vertices, _right)) return false;
            if (Separated(Vertices, box.Vertices, _up)) return false;
            if (Separated(Vertices, box.Vertices, _forward)) return false;

            if (Separated(Vertices, box.Vertices, box._right)) return false;
            if (Separated(Vertices, box.Vertices, box._up)) return false;
            if (Separated(Vertices, box.Vertices, box._forward)) return false;

            if (Separated(Vertices, box.Vertices, Vector3.Cross(_right, box._right))) return false;
            if (Separated(Vertices, box.Vertices, Vector3.Cross(_right, box._up))) return false;
            if (Separated(Vertices, box.Vertices, Vector3.Cross(_right, box._forward))) return false;

            if (Separated(Vertices, box.Vertices, Vector3.Cross(_up, box._right))) return false;
            if (Separated(Vertices, box.Vertices, Vector3.Cross(_up, box._up))) return false;
            if (Separated(Vertices, box.Vertices, Vector3.Cross(_up, box._forward))) return false;

            if (Separated(Vertices, box.Vertices, Vector3.Cross(_forward, box._right))) return false;
            if (Separated(Vertices, box.Vertices, Vector3.Cross(_forward, box._up))) return false;
            if (Separated(Vertices, box.Vertices, Vector3.Cross(_forward, box._forward))) return false;

            return true;
        }

        public bool Intersects(BoundingSphere sphere)
        {
            Matrix4x4.Invert(_matrix, out var inverted);
            var localSpaceSphereCenter = Vector3.Transform(sphere.Center, inverted);
            return new BoundingSphere(localSpaceSphereCenter, sphere.Radius).Intersects(_aaBox);
        }

        public bool Contains(Vector3 point)
        {
            Matrix4x4.Invert(_matrix, out var inverted);
            var localPoint = Vector3.Transform(point, inverted);
            return _aaBox.Contains(localPoint);
        }

        public override string ToString()
        {
            return $"{nameof(_center)}: {_center}, {nameof(_size)}: {_size}";
        }

        private static bool Separated(Vector3[] vertsA, Vector3[] vertsB, Vector3 axis)
        {
            // Handles the cross product = {0,0,0} case
            if (axis == Vector3.Zero)
            {
                return false;
            }

            var aMin = float.MaxValue;
            var aMax = float.MinValue;
            var bMin = float.MaxValue;
            var bMax = float.MinValue;

            // Define two intervals, a and b. Calculate their min and max values
            for (var i = 0; i < 8; i++)
            {
                var aDist = Vector3.Dot(vertsA[i], axis);
                aMin = aDist < aMin ? aDist : aMin;
                aMax = aDist > aMax ? aDist : aMax;
                var bDist = Vector3.Dot(vertsB[i], axis);
                bMin = bDist < bMin ? bDist : bMin;
                bMax = bDist > bMax ? bDist : bMax;
            }

            // One-dimensional intersection test between a and b
            var longSpan = MathF.Max(aMax, bMax) - MathF.Min(aMin, bMin);
            var sumSpan = aMax - aMin + bMax - bMin;
            return longSpan >= sumSpan; // > to treat touching as intersection
        }
    }
}
