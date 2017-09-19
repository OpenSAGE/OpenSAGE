namespace LLGfx
{
    public struct SamplerStateDescription
    {
        public SamplerFilter Filter;
        public int MaxAnisotropy;

        public SamplerStateDescription(SamplerFilter filter)
        {
            Filter = filter;
            MaxAnisotropy = 0;
        }
    }
}
