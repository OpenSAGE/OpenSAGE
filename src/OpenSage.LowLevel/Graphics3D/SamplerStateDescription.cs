namespace OpenSage.LowLevel.Graphics3D
{
    public struct SamplerStateDescription
    {
        public static readonly SamplerStateDescription AnisotropicWrap = new SamplerStateDescription(SamplerFilter.Anisotropic, SamplerAddressMode.Wrap);
        public static readonly SamplerStateDescription LinearWrap = new SamplerStateDescription(SamplerFilter.MinMagMipLinear, SamplerAddressMode.Wrap);
        public static readonly SamplerStateDescription PointWrap = new SamplerStateDescription(SamplerFilter.MinMagMipPoint, SamplerAddressMode.Wrap);

        public static readonly SamplerStateDescription LinearClamp = new SamplerStateDescription(SamplerFilter.MinMagMipLinear, SamplerAddressMode.Clamp);
        public static readonly SamplerStateDescription PointClamp = new SamplerStateDescription(SamplerFilter.MinMagMipPoint, SamplerAddressMode.Clamp);

        public SamplerFilter Filter;
        public int MaxAnisotropy;
        public SamplerAddressMode AddressU;
        public SamplerAddressMode AddressV;

        private SamplerStateDescription(SamplerFilter filter, SamplerAddressMode addressMode)
        {
            Filter = filter;
            AddressU = addressMode;
            AddressV = addressMode;
            MaxAnisotropy = 0;
        }
    }
}
