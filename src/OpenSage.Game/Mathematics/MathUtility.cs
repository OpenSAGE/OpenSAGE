using System;

namespace OpenSage.Mathematics
{
    public static class MathUtility
    {
        public static readonly float Pi = (float) Math.PI;
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

        public static float Cos(float f) => (float) Math.Cos(f);

        public static float Sin(float f) => (float) Math.Sin(f);

        public static float Lerp(float x, float y, float s)
        {
            return x + s * (y - x);
        }
    }
}
