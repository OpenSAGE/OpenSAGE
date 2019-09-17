namespace OpenSage.Mathematics
{
    public readonly struct IntRange
    {
        public readonly int Low;
        public readonly int High;

        public IntRange(int low, int high)
        {
            Low = low;
            High = high;
        }
    }
}
