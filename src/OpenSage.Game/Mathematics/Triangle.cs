using System.Numerics;

namespace OpenSage.Mathematics
{
    public struct Triangle2D
    {
        public Vector2 V0;
        public Vector2 V1;
        public Vector2 V2;
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
