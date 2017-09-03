using LLGfx.Util;
using SharpDX.Direct3D12;
using D3D12 = SharpDX.Direct3D12;

namespace LLGfx
{
    partial class PipelineLayout
    {
        internal RootSignature DeviceRootSignature { get; private set; }

        private void PlatformConstruct(GraphicsDevice graphicsDevice, PipelineLayoutDescription description)
        {
            var inlineDescriptorLayouts = description.InlineDescriptorLayouts ?? new InlineDescriptorLayoutDescription[0];

            var descriptorSetLayouts = description.DescriptorSetLayouts ?? new DescriptorSetLayout[0];

            var rootParameters = new RootParameter[descriptorSetLayouts.Length + inlineDescriptorLayouts.Length];

            for (var i = 0; i < inlineDescriptorLayouts.Length; i++)
            {
                var inlineDescriptorLayout = inlineDescriptorLayouts[i];

                rootParameters[i] = new RootParameter(
                    inlineDescriptorLayout.Visibility.ToShaderVisibility(),
                    new RootDescriptor(inlineDescriptorLayout.ShaderRegister, 0),
                    inlineDescriptorLayout.DescriptorType.ToRootParameterType());
            }

            for (var i = 0; i < descriptorSetLayouts.Length; i++)
            {
                rootParameters[inlineDescriptorLayouts.Length + i] = descriptorSetLayouts[i].DeviceRootParameter;
            }

            var staticSamplerStates = description.StaticSamplerStates ?? new StaticSamplerDescription[0];
            var staticSamplerDescriptions = new D3D12.StaticSamplerDescription[staticSamplerStates.Length];
            for (var i = 0; i < staticSamplerStates.Length; i++)
            {
                var staticSamplerState = staticSamplerStates[i];

                var samplerStateDescription = new D3D12.SamplerStateDescription
                {
                    Filter = staticSamplerState.SamplerStateDescription.Filter.ToFilter(),
                    AddressU = TextureAddressMode.Wrap,
                    AddressV = TextureAddressMode.Wrap,
                    AddressW = TextureAddressMode.Clamp,
                    ComparisonFunction = Comparison.Always,
                    MinimumLod = 0,
                    MaximumLod = float.MaxValue,
                    MaximumAnisotropy = staticSamplerState.SamplerStateDescription.MaxAnisotropy
                };

                staticSamplerDescriptions[i] = new D3D12.StaticSamplerDescription(
                    samplerStateDescription,
                    staticSamplerState.Visibility.ToShaderVisibility(),
                    staticSamplerState.ShaderRegister,
                    0);
            }

            var rootSignatureDescription = new RootSignatureDescription(
                RootSignatureFlags.AllowInputAssemblerInputLayout,
                parameters: rootParameters,
                samplers: staticSamplerDescriptions);

            var serializedRootSignatureDescription = rootSignatureDescription.Serialize();
            DeviceRootSignature = AddDisposable(graphicsDevice.Device.CreateRootSignature(serializedRootSignatureDescription));
        }
    }
}
