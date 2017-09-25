using System.Numerics;

namespace OpenSage.Mathematics
{
    public static class Vector3Utility
    {
        public static Vector3 Lerp(ref Vector3 x, ref Vector3 y, float s)
        {
            return x + s * (y - x);
        }
    }
}
