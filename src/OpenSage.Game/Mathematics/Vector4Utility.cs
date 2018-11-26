using System;
using System.Numerics;

namespace OpenSage.Mathematics
{
    public static class Vector4Utility
    {
        public static Vector4 Round(in Vector4 value)
        {
            return new Vector4(
                (float) Math.Round(value.X),
                (float) Math.Round(value.Y),
                (float) Math.Round(value.Z),
                (float) Math.Round(value.W));
        }

        public static Vector3 ToVector3(this in Vector4 value)
        {
            return new Vector3(value.X, value.Y, value.Z) / value.W;
        }
    }
}
