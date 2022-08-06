using System.Numerics;

namespace OpenSage.Mathematics
{
    public readonly struct Triangle2D
    {
        public readonly Vector2 V0;
        public readonly Vector2 V1;
        public readonly Vector2 V2;

        public Triangle2D(in Vector2 v0, in Vector2 v1, in Vector2 v2)
        {
            V0 = v0;
            V1 = v1;
            V2 = v2;
        }

        public static Triangle2D Transform(in Triangle2D triangle, in Matrix3x2 matrix)
        {
            return new Triangle2D(
                Vector2.Transform(triangle.V0, matrix),
                Vector2.Transform(triangle.V1, matrix),
                Vector2.Transform(triangle.V2, matrix));
        }
    }

    public readonly struct Triangle
    {
        public readonly Vector3 V0;
        public readonly Vector3 V1;
        public readonly Vector3 V2;

        public Triangle(in Vector3 v0, in Vector3 v1, in Vector3 v2)
        {
            V0 = v0;
            V1 = v1;
            V2 = v2;
        }
    }

    public readonly struct IndexedTriangle
    {
        public readonly ushort IDX0;
        public readonly ushort IDX1;
        public readonly ushort IDX2;

        public IndexedTriangle(ushort i0, ushort i1, ushort i2)
        {
            IDX0 = i0;
            IDX1 = i1;
            IDX2 = i2;
        }
    }
}
