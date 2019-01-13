using System;

namespace OpenSage.Utilities
{
    public static class TimeSpanUtility
    {
        public static TimeSpan Max(TimeSpan a, TimeSpan b)
        {
            return a > b ? a : b;
        }
    }
}
