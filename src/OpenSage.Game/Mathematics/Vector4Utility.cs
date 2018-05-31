using System.Numerics;

namespace OpenSage.Mathematics
{
    public static class Vector4Utility
    {
        public static Vector3 ToVector3(this Vector4 value)
        {
            return new Vector3(value.X, value.Y, value.Z) / value.W;
        }

        public static Vector4 Round(in Vector4 value)
        {
            return new Vector4(
                MathUtility.Round(value.X),
                MathUtility.Round(value.Y),
                MathUtility.Round(value.Z),
                MathUtility.Round(value.W));
        }
    }
}
