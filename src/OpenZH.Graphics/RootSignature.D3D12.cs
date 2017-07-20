using D3D12 = SharpDX.Direct3D12;

namespace OpenZH.Graphics
{
    partial class RootSignature
    {
        internal D3D12.RootSignature DeviceRootSignature { get; private set; }

        private void PlatformConstruct(GraphicsDevice graphicsDevice, RootSignatureDescriptor descriptor)
        {
            var serializedRootSignatureDescription = descriptor.DeviceDescription.Serialize();
            DeviceRootSignature = AddDisposable(graphicsDevice.Device.CreateRootSignature(serializedRootSignatureDescription));
        }
    }
}
