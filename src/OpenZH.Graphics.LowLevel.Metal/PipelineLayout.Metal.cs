using System;
using Metal;

namespace OpenZH.Graphics.LowLevel
{
    partial class PipelineLayout
    {
        public IMTLSamplerState[] DeviceSamplerStates { get; private set; }

        private void PlatformConstruct(GraphicsDevice graphicsDevice, PipelineLayoutDescription description)
        {
            DeviceSamplerStates = new IMTLSamplerState[description.StaticSamplerStates.Length];

            for (var i = 0; i < description.StaticSamplerStates.Length; i++)
            {
                var staticSamplerState = description.StaticSamplerStates[i];

                DeviceSamplerStates[i] = graphicsDevice.Device.CreateSamplerState(new MTLSamplerDescriptor
                {
                    RAddressMode = MTLSamplerAddressMode.ClampToEdge,
                    SAddressMode = MTLSamplerAddressMode.ClampToEdge,
                    TAddressMode = MTLSamplerAddressMode.ClampToEdge,
                    MinFilter = ToMinMagFilter(staticSamplerState.SamplerStateDescription.Filter),
                    MagFilter = ToMinMagFilter(staticSamplerState.SamplerStateDescription.Filter),
                    MipFilter = ToMipFilter(staticSamplerState.SamplerStateDescription.Filter),
                    MaxAnisotropy = (nuint) staticSamplerState.SamplerStateDescription.MaxAnisotropy
                });
            }
        }

        private static MTLSamplerMinMagFilter ToMinMagFilter(SamplerFilter value)
        {
            switch (value)
            {
                case SamplerFilter.MinMagMipPoint:
                    return MTLSamplerMinMagFilter.Nearest;

                case SamplerFilter.MinMagMipLinear:
                    return MTLSamplerMinMagFilter.Linear;

                case SamplerFilter.Anisotropic:
                    return MTLSamplerMinMagFilter.Linear; // TODO

                default:
                    throw new ArgumentOutOfRangeException(nameof(value));
            }
        }

        private static MTLSamplerMipFilter ToMipFilter(SamplerFilter value)
        {
            switch (value)
            {
                case SamplerFilter.MinMagMipPoint:
                    return MTLSamplerMipFilter.Nearest;

                case SamplerFilter.MinMagMipLinear:
                    return MTLSamplerMipFilter.Linear;

                case SamplerFilter.Anisotropic:
                    return MTLSamplerMipFilter.NotMipmapped;

                default:
                    throw new ArgumentOutOfRangeException(nameof(value));
            }
        }
    }
}
