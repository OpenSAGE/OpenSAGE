using System.Numerics;

namespace OpenSage.Mathematics
{
    // From the MonoGame implementation, licenced under the MIT licence:
    // https://github.com/MonoGame/MonoGame/blob/develop/MonoGame.Framework/BoundingFrustum.cs
    public sealed class BoundingFrustum
    {
        public const int PlaneCount = 6;
        public const int CornerCount = 8;
        private readonly Plane[] _planes = new Plane[PlaneCount];

        private Matrix4x4 _matrix;

        public BoundingFrustum(in Matrix4x4 value)
        {
            _matrix = value;
            CreatePlanes();
            CreateCorners();
        }

        public Vector3[] Corners { get; } = new Vector3[CornerCount];

        public Matrix4x4 Matrix
        {
            get => _matrix;
            set
            {
                _matrix = value;
                CreatePlanes();
                CreateCorners();
            }
        }

        public static bool operator ==(BoundingFrustum a, BoundingFrustum b)
        {
            if (Equals(a, null))
            {
                return Equals(b, null);
            }

            if (Equals(b, null))
            {
                return false;
            }

            return a._matrix == b._matrix;
        }

        public static bool operator !=(BoundingFrustum a, BoundingFrustum b)
        {
            return !(a == b);
        }

        public bool Equals(BoundingFrustum other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            return obj is BoundingFrustum frustum && this == frustum;
        }

        public override int GetHashCode()
        {
            return _matrix.GetHashCode();
        }

        public bool Contains(in Vector3 point)
        {
            for (var i = 0; i < PlaneCount; ++i)
            {
                if (ClassifyPoint(point, _planes[i]) > 0)
                {
                    return false;
                }
            }

            return true;
        }

        public ContainmentType Contains(in AxisAlignedBoundingBox box)
        {
            var intersects = false;
            for (var i = 0; i < PlaneCount; ++i)
            {
                var planeIntersectionType = box.Intersects(_planes[i]);
                switch (planeIntersectionType)
                {
                    case PlaneIntersectionType.Front:
                        return ContainmentType.Disjoint;

                    case PlaneIntersectionType.Intersecting:
                        intersects = true;
                        break;
                }
            }

            return intersects
                ? ContainmentType.Intersects
                : ContainmentType.Contains;
        }

        public ContainmentType Contains(in BoundingSphere sphere)
        {
            var intersects = false;
            for (var i = 0; i < PlaneCount; ++i)
            {
                var planeIntersectionType = sphere.Intersects(_planes[i]);
                switch (planeIntersectionType)
                {
                    case PlaneIntersectionType.Front:
                        return ContainmentType.Disjoint;

                    case PlaneIntersectionType.Intersecting:
                        intersects = true;
                        break;
                }
            }

            return intersects
                ? ContainmentType.Intersects
                : ContainmentType.Contains;
        }

        public bool Intersects(in AxisAlignedBoundingBox box)
        {
            return Contains(box) != ContainmentType.Disjoint;
        }

        public bool Intersects(in BoundingSphere sphere)
        {
            return Contains(sphere) != ContainmentType.Disjoint;
        }

        private void CreateCorners()
        {
            IntersectionPoint(_planes[0], _planes[2], _planes[4], out Corners[0]);
            IntersectionPoint(_planes[0], _planes[3], _planes[4], out Corners[1]);
            IntersectionPoint(_planes[0], _planes[3], _planes[5], out Corners[2]);
            IntersectionPoint(_planes[0], _planes[2], _planes[5], out Corners[3]);
            IntersectionPoint(_planes[1], _planes[2], _planes[4], out Corners[4]);
            IntersectionPoint(_planes[1], _planes[3], _planes[4], out Corners[5]);
            IntersectionPoint(_planes[1], _planes[3], _planes[5], out Corners[6]);
            IntersectionPoint(_planes[1], _planes[2], _planes[5], out Corners[7]);
        }

        private void CreatePlanes()
        {
            _planes[0] = new Plane(-_matrix.M13, -_matrix.M23, -_matrix.M33, -_matrix.M43);
            _planes[1] = new Plane(_matrix.M13 - _matrix.M14, _matrix.M23 - _matrix.M24, _matrix.M33 - _matrix.M34,
                _matrix.M43 - _matrix.M44);
            _planes[2] = new Plane(-_matrix.M14 - _matrix.M11, -_matrix.M24 - _matrix.M21, -_matrix.M34 - _matrix.M31,
                -_matrix.M44 - _matrix.M41);
            _planes[3] = new Plane(_matrix.M11 - _matrix.M14, _matrix.M21 - _matrix.M24, _matrix.M31 - _matrix.M34,
                _matrix.M41 - _matrix.M44);
            _planes[4] = new Plane(_matrix.M12 - _matrix.M14, _matrix.M22 - _matrix.M24, _matrix.M32 - _matrix.M34,
                _matrix.M42 - _matrix.M44);
            _planes[5] = new Plane(-_matrix.M14 - _matrix.M12, -_matrix.M24 - _matrix.M22, -_matrix.M34 - _matrix.M32,
                -_matrix.M44 - _matrix.M42);

            NormalizePlane(ref _planes[0]);
            NormalizePlane(ref _planes[1]);
            NormalizePlane(ref _planes[2]);
            NormalizePlane(ref _planes[3]);
            NormalizePlane(ref _planes[4]);
            NormalizePlane(ref _planes[5]);
        }

        private static void IntersectionPoint(in Plane a, in Plane b, in Plane c, out Vector3 result)
        {
            // Formula used
            //                d1 ( N2 * N3 ) + d2 ( N3 * N1 ) + d3 ( N1 * N2 )
            //P =   -------------------------------------------------------------------------
            //                             N1 . ( N2 * N3 )
            //
            // Note: N refers to the normal, d refers to the displacement. '.' means dot product. '*' means cross product

            var cross = Vector3.Cross(b.Normal, c.Normal);

            var f = Vector3.Dot(a.Normal, cross);
            f *= -1.0f;

            cross = Vector3.Cross(b.Normal, c.Normal);
            var v1 = Vector3.Multiply(cross, a.D);

            cross = Vector3.Cross(c.Normal, a.Normal);
            var v2 = Vector3.Multiply(cross, b.D);

            cross = Vector3.Cross(a.Normal, b.Normal);
            var v3 = Vector3.Multiply(cross, c.D);

            result.X = (v1.X + v2.X + v3.X) / f;
            result.Y = (v1.Y + v2.Y + v3.Y) / f;
            result.Z = (v1.Z + v2.Z + v3.Z) / f;
        }

        private void NormalizePlane(ref Plane p)
        {
            var factor = 1f / p.Normal.Length();
            p.Normal.X *= factor;
            p.Normal.Y *= factor;
            p.Normal.Z *= factor;
            p.D *= factor;
        }

        // Ported from MonoGame:
        // https://github.com/MonoGame/MonoGame/blob/e517aa9a4449cabf15da6ffad8dc5ebbf0ac4c5f/MonoGame.Framework/Plane.cs#L19
        /// <summary>
        ///     Returns a value indicating what side (positive/negative) of a plane a point is
        /// </summary>
        /// <param name="point">The point to check with</param>
        /// <param name="plane">The plane to check against</param>
        /// <returns>Greater than zero if on the positive side, less than zero if on the negative size, 0 otherwise</returns>
        private static float ClassifyPoint(in Vector3 point, in Plane plane)
        {
            return point.X * plane.Normal.X + point.Y * plane.Normal.Y + point.Z * plane.Normal.Z + plane.D;
        }
    }
}
