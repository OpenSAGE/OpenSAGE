using OpenSage.LowLevel.Graphics3D.Util;
using Metal;
using System;

namespace OpenSage.LowLevel.Graphics3D
{
    partial class SamplerState
    {
        internal IMTLSamplerState DeviceSamplerState { get; private set; }

        internal override string PlatformGetDebugName() => DeviceSamplerState.Label;
        internal override void PlatformSetDebugName(string value) { }

        private void PlatformConstruct(GraphicsDevice graphicsDevice, SamplerStateDescription description)
        {
            using (var samplerDescriptor = ToMTLSamplerDescriptor(description))
            {
                DeviceSamplerState = AddDisposable(graphicsDevice.Device.CreateSamplerState(samplerDescriptor));
            }
        }

        private static MTLSamplerDescriptor ToMTLSamplerDescriptor(in SamplerStateDescription description)
        {
            description.Filter.ToMTLSamplerFilters(
                out var minFilter,
                out var magFilter,
                out var mipFilter);

            return new MTLSamplerDescriptor
            {
                MinFilter = minFilter,
                MagFilter = magFilter,
                MipFilter = mipFilter,
                RAddressMode = description.AddressU.ToMTLSamplerAddressMode(),
                SAddressMode = description.AddressV.ToMTLSamplerAddressMode(),
                TAddressMode = MTLSamplerAddressMode.ClampToEdge,
                CompareFunction = MTLCompareFunction.Always,
                LodMinClamp = 0,
                LodMaxClamp = float.MaxValue,
                MaxAnisotropy = (nuint) description.MaxAnisotropy
            };
        }
    }
}
