using System.Linq;
using SharpDX.Direct3D12;

namespace OpenZH.Graphics
{
    partial class PipelineLayout
    {
        internal RootSignature DeviceRootSignature { get; private set; }

        private void PlatformConstruct(GraphicsDevice graphicsDevice, PipelineLayoutDescription description)
        {
            var rootSignatureDescription = new RootSignatureDescription(
                RootSignatureFlags.AllowInputAssemblerInputLayout,
                parameters: description.DescriptorSetLayouts.Select(x => x.DeviceRootParameter).ToArray());

            var serializedRootSignatureDescription = rootSignatureDescription.Serialize();
            DeviceRootSignature = AddDisposable(graphicsDevice.Device.CreateRootSignature(serializedRootSignatureDescription));
        }
    }
}
