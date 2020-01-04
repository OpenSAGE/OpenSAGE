using System;
using System.Numerics;

namespace OpenSage.Mathematics
{
    public static class MathUtility
    {
        public static readonly float Pi = (float) Math.PI;
        public static readonly float PiOver2 = Pi / 2;
        public static readonly float TwoPi = Pi * 2;

        public static float Sqrt(float v) => (float) Math.Sqrt(v);

        public static float Pow(float x, float y) => (float) Math.Pow(x, y);

        public static int FloorToInt(float f)
        {
            return (int) Math.Floor(f);
        }

        public static int Clamp(int value, int min, int max)
        {
            value = (value > max) ? max : value;
            value = (value < min) ? min : value;
            return value;
        }

        public static float Clamp(float value, float min, float max)
        {
            value = (value > max) ? max : value;
            value = (value < min) ? min : value;
            return value;
        }

        public static float Cos(float f) => (float) Math.Cos(f);

        public static float Sin(float f) => (float) Math.Sin(f);

        public static float Atan(float f) => (float) Math.Atan(f);

        /// <summary>
        /// Returns the angle whose tangent is the quotient of the two specified numbers.
        /// </summary>
        public static float Atan2(float y, float x)
        {
            return (float) Math.Atan2(y, x);
        }

        /// <summary>
        /// Returns the angle whose sine is the specified number.
        /// </summary>
        public static float Asin(float a)
        {
            return (float) Math.Asin(a);
        }

        /// <summary>
        /// Returns the angle whose cosine is the specified number.
        /// </summary>
        public static float Acos(float a)
        {
            return (float) Math.Acos(a);
        }

        public static float Lerp(float x, float y, float s)
        {
            return x + s * (y - x);
        }

        public static float ToRadians(float degrees)
        {
            return (degrees * Pi / 180);
        }

        public static float ToDegrees(float radians)
        {
            return (radians * 180 / Pi);
        }

        /// <summary>
        /// Calculates the angle z vector starting from UnitX
        /// </summary>
        /// <param name="direction">the direction vector</param>
        /// <returns>the z angle of the direction vector (in radians)</returns>
        public static float GetZAngleFromDirection(Vector3 direction)
        {
            return MathF.Atan2(direction.Y, direction.X) - MathF.Atan2(Vector2.UnitX.Y, Vector2.UnitX.X);
        }

        /// <summary>
        /// Calculates delta between the 2 angles (which must be between -180 and 180)
        /// </summary>
        /// <param name="alpha">the first angle</param>
        /// <param name="beta">the second angle</param>
        /// <returns>the absolute delta between the two angles in radians.
        /// The value is between -180 and 180 degrees by definition.
        /// </returns>
        public static float CalculateAngleDelta(float alpha, float beta)
        {
            var delta = beta - alpha;
            delta += (delta > Pi) ? -TwoPi : (delta < -Pi) ? TwoPi : 0;
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
