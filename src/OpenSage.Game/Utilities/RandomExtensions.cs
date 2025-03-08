using System;

namespace OpenSage.Utilities;

internal static class RandomExtensions
{
    public static float NextSingle(this Random random, float lo, float hi)
    {
        var delta = hi - lo;

        if (delta <= 0.0f)
        {
            return hi;
        }

        return random.NextSingle() * delta + lo;
    }
}
