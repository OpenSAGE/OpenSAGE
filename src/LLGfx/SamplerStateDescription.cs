namespace LLGfx
{
    public struct SamplerStateDescription
    {
        public static readonly SamplerStateDescription AnisotropicWrap = new SamplerStateDescription(SamplerFilter.Anisotropic);
        public static readonly SamplerStateDescription LinearWrap = new SamplerStateDescription(SamplerFilter.MinMagMipLinear);
        public static readonly SamplerStateDescription PointWrap = new SamplerStateDescription(SamplerFilter.MinMagMipPoint);

        public SamplerFilter Filter;
        public int MaxAnisotropy;
        public SamplerAddressMode AddressU;
        public SamplerAddressMode AddressV;

        private SamplerStateDescription(SamplerFilter filter)
        {
            Filter = filter;
            AddressU = SamplerAddressMode.Wrap;
            AddressV = SamplerAddressMode.Wrap;
            MaxAnisotropy = 0;
        }
    }
}
