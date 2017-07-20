using SharpDX.Direct3D12;

namespace OpenZH.Graphics
{
    partial class Buffer
    {
        internal Resource DeviceBuffer { get; private set; }

        private void PlatformConstruct(GraphicsDevice graphicsDevice, uint sizeInBytes)
        {
            DeviceBuffer = AddDisposable(graphicsDevice.Device.CreateCommittedResource(
                new HeapProperties(HeapType.Upload),
                HeapFlags.None,
                ResourceDescription.Buffer(sizeInBytes),
                ResourceStates.GenericRead));
        }
    }
}
