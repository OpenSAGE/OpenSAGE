using System;

namespace OpenSage.Mathematics
{
    public readonly struct FloatRange
    {
        public readonly float Low;
        public readonly float High;

        public FloatRange(float low, float high)
        {
            Low = low;
            High = high;
        }

        public float GetValue(Random random) => (float)(random.NextDouble() * (High - Low) + Low);
    }
}
