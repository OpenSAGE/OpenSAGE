using LL.Graphics3D.Util;
using SharpDX.Direct3D11;
using D3D11 = SharpDX.Direct3D11;

namespace LL.Graphics3D
{
    partial class SamplerState
    {
        internal D3D11.SamplerState DeviceSamplerState { get; private set; }

        internal override string PlatformGetDebugName() => DeviceSamplerState.DebugName;
        internal override void PlatformSetDebugName(string value) => DeviceSamplerState.DebugName = value;

        private void PlatformConstruct(GraphicsDevice graphicsDevice, SamplerStateDescription description)
        {
            DeviceSamplerState = AddDisposable(new D3D11.SamplerState(
                graphicsDevice.Device,
                new D3D11.SamplerStateDescription
                {
                    Filter = description.Filter.ToFilter(),
                    AddressU = description.AddressU.ToTextureAddressMode(),
                    AddressV = description.AddressV.ToTextureAddressMode(),
                    AddressW = TextureAddressMode.Clamp,
                    ComparisonFunction = D3D11.Comparison.Always,
                    MinimumLod = 0,
                    MaximumLod = float.MaxValue,
                    MaximumAnisotropy = description.MaxAnisotropy
                }));
        }
    }
}
