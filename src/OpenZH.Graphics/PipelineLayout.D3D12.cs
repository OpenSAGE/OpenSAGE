using System.Linq;
using OpenZH.Graphics.Platforms.Direct3D12;
using SharpDX.Direct3D12;
using D3D12 = SharpDX.Direct3D12;

namespace OpenZH.Graphics
{
    partial class PipelineLayout
    {
        internal RootSignature DeviceRootSignature { get; private set; }

        private void PlatformConstruct(GraphicsDevice graphicsDevice, PipelineLayoutDescription description)
        {
            var staticSamplerStates = description.StaticSamplerStates ?? new StaticSamplerDescription[0];
            var staticSamplerDescriptions = staticSamplerStates.Select(x =>
            {
                var samplerStateDescription = new D3D12.SamplerStateDescription
                {
                    Filter = x.SamplerStateDescription.Filter.ToFilter(),
                    AddressU = TextureAddressMode.Clamp,
                    AddressV = TextureAddressMode.Clamp,
                    AddressW = TextureAddressMode.Clamp,
                    ComparisonFunction = Comparison.Always,
                    MinimumLod = 0,
                    MaximumLod = float.MaxValue,
                    MaximumAnisotropy = x.SamplerStateDescription.MaxAnisotropy
                };

                return new D3D12.StaticSamplerDescription(
                    samplerStateDescription,
                    x.Visibility.ToShaderVisibility(),
                    x.ShaderRegister,
                    0);
            });

            var rootSignatureDescription = new RootSignatureDescription(
                RootSignatureFlags.AllowInputAssemblerInputLayout,
                parameters: description.DescriptorSetLayouts.Select(x => x.DeviceRootParameter).ToArray(),
                samplers: staticSamplerDescriptions.ToArray());

            var serializedRootSignatureDescription = rootSignatureDescription.Serialize();
            DeviceRootSignature = AddDisposable(graphicsDevice.Device.CreateRootSignature(serializedRootSignatureDescription));
        }
    }
}
