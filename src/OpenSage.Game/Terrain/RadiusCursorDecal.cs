using System.Numerics;

namespace OpenSage.Terrain
{
    internal struct RadiusCursorDecal
    {
        public const uint SizeInBytes = 32;

        public Vector2 BottomLeftCornerPosition;
        public float Diameter;

        public uint DecalTextureIndex;

#pragma warning disable IDE0051 // Remove unused private members
        private readonly Vector3 _padding;
#pragma warning restore IDE0051 // Remove unused private members

        public float Opacity;
    }
}
