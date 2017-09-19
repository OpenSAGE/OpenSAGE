using System;

namespace OpenSage.Mathematics
{
    public static class MathUtility
    {
        public static readonly float Pi = (float) Math.PI;

        public static float Sqrt(float v) => (float) Math.Sqrt(v);

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
    }
}
