using System;
using System.Numerics;

namespace OpenSage.Mathematics
{
    public static class MathUtility
    {
        public static readonly float PiOver4 = MathF.PI / 4.0f;
        public static readonly float PiOver2 = MathF.PI / 2.0f;
        public static readonly float TwoPi = MathF.PI * 2.0f;
        public static readonly float DegreesToRadiansRatio = MathF.PI / 180;

        public static int FloorToInt(float f) => (int) MathF.Floor(f);

        public static float Lerp(float x, float y, float s)
        {
            return x + s * (y - x);
        }

        public static float ToRadians(float degrees)
        {
            return (degrees * MathF.PI / 180);
        }

        public static float ToDegrees(float radians)
        {
            return (radians * 180 / MathF.PI);
        }

        /// <summary>
        /// Calculates the angle z vector starting from UnitX
        /// </summary>
        /// <param name="direction">the direction vector</param>
        /// <returns>the z angle of the direction vector (in radians)</returns>
        public static float GetYawFromDirection(Vector2 direction)
        {
            return MathF.Atan2(direction.Y, direction.X) - MathF.Atan2(Vector2.UnitX.Y, Vector2.UnitX.X);
        }

        public static float GetPitchFromDirection(in Vector3 direction)
        {
            return MathF.Acos(Vector3.Dot(direction, Vector3.UnitZ) / direction.Length());
        }

        /// <summary>
        /// Calculates delta between the 2 angles (which must be between -180 and 180)
        /// </summary>
        /// <param name="alpha">the first angle</param>
        /// <param name="beta">the second angle</param>
        /// <returns>the absolute delta between the two angles in radians.
        /// The value is between -PI and PI degrees by definition.
        /// </returns>
        public static float CalculateAngleDelta(float alpha, float beta)
        {
            var delta = beta - alpha;
            delta += (delta > MathF.PI) ? -TwoPi : (delta < -MathF.PI) ? TwoPi : 0;
            return delta;
        }

        public static uint NextPowerOfTwo(uint value)
        {
            value--;
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;
            value++;

            return value;
        }
    }
}
