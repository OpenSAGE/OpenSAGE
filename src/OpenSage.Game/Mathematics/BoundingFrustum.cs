using System.Numerics;

namespace OpenSage.Mathematics
{
    // From the MonoGame implementation, licenced under the MIT licence:
    // https://github.com/MonoGame/MonoGame/blob/develop/MonoGame.Framework/BoundingFrustum.cs
    public sealed class BoundingFrustum
    {
        public const int PlaneCount = 6;
        public const int CornerCount = 8;

        private Matrix4x4 _matrix;
        private readonly Vector3[] _corners = new Vector3[CornerCount];
        private readonly Plane[] _planes = new Plane[PlaneCount];

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

        public BoundingFrustum(in Matrix4x4 value)
        {
            _matrix = value;
            CreatePlanes();
            CreateCorners();
        }

        public static bool operator ==(BoundingFrustum a, BoundingFrustum b)
        {
            if (Equals(a, null))
                return (Equals(b, null));

            if (Equals(b, null))
                return (Equals(a, null));

            return a._matrix == (b._matrix);
        }

        public static bool operator !=(BoundingFrustum a, BoundingFrustum b)
        {
            return !(a == b);
        }

        public bool Equals(BoundingFrustum other)
        {
            return (this == other);
        }

        public override bool Equals(object obj)
        {
            return (obj is BoundingFrustum) && this == ((BoundingFrustum) obj);
        }

        public override int GetHashCode() => _matrix.GetHashCode();

        public ContainmentType Contains(in BoundingBox box)
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

        public bool Intersects(in BoundingBox box)
        {
            return Contains(box) != ContainmentType.Disjoint;
        }

        public bool Intersects(in BoundingSphere sphere)
        {
            return Contains(sphere) != ContainmentType.Disjoint;
        }

        private void CreateCorners()
        {
            IntersectionPoint(this._planes[0], this._planes[2], this._planes[4], out this._corners[0]);
            IntersectionPoint(this._planes[0], this._planes[3], this._planes[4], out this._corners[1]);
            IntersectionPoint(this._planes[0], this._planes[3], this._planes[5], out this._corners[2]);
            IntersectionPoint(this._planes[0], this._planes[2], this._planes[5], out this._corners[3]);
            IntersectionPoint(this._planes[1], this._planes[2], this._planes[4], out this._corners[4]);
            IntersectionPoint(this._planes[1], this._planes[3], this._planes[4], out this._corners[5]);
            IntersectionPoint(this._planes[1], this._planes[3], this._planes[5], out this._corners[6]);
            IntersectionPoint(this._planes[1], this._planes[2], this._planes[5], out this._corners[7]);
        }

        private void CreatePlanes()
        {
            this._planes[0] = new Plane(-this._matrix.M13, -this._matrix.M23, -this._matrix.M33, -this._matrix.M43);
            this._planes[1] = new Plane(this._matrix.M13 - this._matrix.M14, this._matrix.M23 - this._matrix.M24, this._matrix.M33 - this._matrix.M34, this._matrix.M43 - this._matrix.M44);
            this._planes[2] = new Plane(-this._matrix.M14 - this._matrix.M11, -this._matrix.M24 - this._matrix.M21, -this._matrix.M34 - this._matrix.M31, -this._matrix.M44 - this._matrix.M41);
            this._planes[3] = new Plane(this._matrix.M11 - this._matrix.M14, this._matrix.M21 - this._matrix.M24, this._matrix.M31 - this._matrix.M34, this._matrix.M41 - this._matrix.M44);
            this._planes[4] = new Plane(this._matrix.M12 - this._matrix.M14, this._matrix.M22 - this._matrix.M24, this._matrix.M32 - this._matrix.M34, this._matrix.M42 - this._matrix.M44);
            this._planes[5] = new Plane(-this._matrix.M14 - this._matrix.M12, -this._matrix.M24 - this._matrix.M22, -this._matrix.M34 - this._matrix.M32, -this._matrix.M44 - this._matrix.M42);

            this.NormalizePlane(ref this._planes[0]);
            this.NormalizePlane(ref this._planes[1]);
            this.NormalizePlane(ref this._planes[2]);
            this.NormalizePlane(ref this._planes[3]);
            this.NormalizePlane(ref this._planes[4]);
            this.NormalizePlane(ref this._planes[5]);
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
            float factor = 1f / p.Normal.Length();
            p.Normal.X *= factor;
            p.Normal.Y *= factor;
            p.Normal.Z *= factor;
            p.D *= factor;
        }
    }
}
