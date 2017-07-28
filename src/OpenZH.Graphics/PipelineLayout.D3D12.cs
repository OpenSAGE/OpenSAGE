using System.Linq;
using SharpDX.Direct3D12;

namespace OpenZH.Graphics
{
    partial class PipelineLayout
    {
        internal RootSignature DeviceRootSignature { get; private set; }

        private void PlatformConstruct(GraphicsDevice graphicsDevice, PipelineLayoutDescription description)
        {
            // TODO
            var samplerStateDescription = new SamplerStateDescription
            {
                Filter = Filter.MinMagMipLinear,
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                ComparisonFunction = Comparison.Always,
                MinimumLod = 0,
                MaximumLod = float.MaxValue
            };
            var staticSamplerDescription = new StaticSamplerDescription(
                samplerStateDescription,
                ShaderVisibility.Pixel,
                0, 0);

            var rootSignatureDescription = new RootSignatureDescription(
                RootSignatureFlags.AllowInputAssemblerInputLayout,
                parameters: description.DescriptorSetLayouts.Select(x => x.DeviceRootParameter).ToArray(),
                samplers: new[] { staticSamplerDescription });

            var serializedRootSignatureDescription = rootSignatureDescription.Serialize();
            DeviceRootSignature = AddDisposable(graphicsDevice.Device.CreateRootSignature(serializedRootSignatureDescription));
        }
    }
}
