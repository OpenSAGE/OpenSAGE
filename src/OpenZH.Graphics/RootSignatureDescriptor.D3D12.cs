using SharpDX.Direct3D12;

namespace OpenZH.Graphics
{
    partial class RootSignatureDescriptor
    {
        internal RootSignatureDescription DeviceDescription { get; private set; }

        private void PlatformConstruct()
        {
            DeviceDescription = new RootSignatureDescription(
                RootSignatureFlags.AllowInputAssemblerInputLayout);
        }

        private void PlatformSetParameter()
        {
            // TODO
        }
    }
}
