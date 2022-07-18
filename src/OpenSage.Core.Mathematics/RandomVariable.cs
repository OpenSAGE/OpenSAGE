namespace OpenSage.Mathematics
{
    public readonly record struct RandomVariable(float Low, float High, DistributionType DistributionType)
    {
        public override string ToString()
        {
            return $"{Low},{High} ({DistributionType})";
        }
    }

    public enum DistributionType
    {
        Constant,
        Uniform
    }
}
