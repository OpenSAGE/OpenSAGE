namespace LLGfx
{
    public struct SamplerStateDescription
    {
        public static readonly SamplerStateDescription Default = new SamplerStateDescription
        {
            Filter = SamplerFilter.MinMagMipLinear,
            AddressU = SamplerAddressMode.Wrap,
            AddressV = SamplerAddressMode.Wrap,
            MaxAnisotropy = 0
        };

        public SamplerFilter Filter;
        public int MaxAnisotropy;
        public SamplerAddressMode AddressU;
        public SamplerAddressMode AddressV;
    }
}
