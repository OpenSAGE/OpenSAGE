using System.Numerics;

namespace OpenSage.Mathematics
{
    public enum PlaneIntersectionType
    {
        Front,
        Back,
        Intersecting
    }

    public static class PlaneExtensions
    {
        public static Vector4 AsVector4(this in Plane plane)
        {
            return new Vector4(plane.Normal, plane.D);
        }
    }
}
