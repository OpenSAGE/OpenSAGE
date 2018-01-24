using System.Numerics;

namespace OpenSage.Mathematics
{
    public struct Triangle2D
    {
        public Vector2 V0;
        public Vector2 V1;
        public Vector2 V2;

        public Triangle2D(in Vector2 v0, in Vector2 v1, in Vector2 v2)
        {
            V0 = v0;
            V1 = v1;
            V2 = v2;
        }

        public static Triangle2D Transform(in Triangle2D triangle, in Matrix3x2 matrix)
        {
            return new Triangle2D
            {
                V0 = Vector2.Transform(triangle.V0, matrix),
                V1 = Vector2.Transform(triangle.V1, matrix),
                V2 = Vector2.Transform(triangle.V2, matrix)
            };
        }
    }

    public struct Triangle
    {
        public Vector3 V0;
        public Vector3 V1;
        public Vector3 V2;
    }

    public struct IndexedTriangle
    {
        public ushort IDX0;
        public ushort IDX1;
        public ushort IDX2;

        public IndexedTriangle(ushort i0,ushort i1,ushort i2)
        {
            IDX0 = i0;
            IDX1 = i1;
            IDX2 = i2;
        }
    }
}
