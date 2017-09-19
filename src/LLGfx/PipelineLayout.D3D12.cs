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
            var rootParameters = new RootParameter[description.Entries.Length];

            for (var i = 0; i < description.Entries.Length; i++)
            {
                var entry = description.Entries[i];

                switch (entry.EntryType)
                {
                    case PipelineLayoutEntryType.Resource:
                        rootParameters[i] = new RootParameter(
                            entry.Visibility.ToShaderVisibility(),
                            new RootDescriptor(entry.Resource.ShaderRegister, 0),
                            entry.ResourceType.ToRootParameterType());
                        break;

                    case PipelineLayoutEntryType.ResourceView:
                        rootParameters[i] = new RootParameter(
                            entry.Visibility.ToShaderVisibility(),
                            new DescriptorRange(
                                entry.ResourceType.ToDescriptorRangeType(),
                                entry.ResourceView.ResourceCount,
                                entry.ResourceView.BaseShaderRegister));
                        break;

                    default:
                        throw new System.InvalidOperationException();
                }
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
