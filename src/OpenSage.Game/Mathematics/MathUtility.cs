using System;

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

        internal static uint NextPowerOfTwo(uint value)
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
