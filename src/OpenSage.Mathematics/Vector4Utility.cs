using System;
using System.Numerics;

namespace OpenSage.Mathematics
{
    public static class Vector4Utility
    {
        public static Vector4 Round(in Vector4 value)
        {
            return new Vector4(
                MathF.Round(value.X),
                MathF.Round(value.Y),
                MathF.Round(value.Z),
                MathF.Round(value.W));
        }

        public static Vector3 ToVector3(this in Vector4 value)
        {
            return new Vector3(value.X, value.Y, value.Z) / value.W;
        }
    }
}
