namespace OpenSage.Mathematics
{
    public readonly struct RandomVariable
    {
        public readonly float Low;
        public readonly float High;
        public readonly DistributionType DistributionType;

        public RandomVariable(float low, float high, DistributionType distributionType)
        {
            Low = low;
            High = high;
            DistributionType = distributionType;
        }
    }

    public enum DistributionType
    {
        Constant,
        Uniform
    }
}
