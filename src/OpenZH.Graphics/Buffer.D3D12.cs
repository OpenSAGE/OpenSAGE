using SharpDX;
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

        private void PlatformSetData<T>(T data, int offset)
            where T : struct
        {
            // TODO: Add API to keep buffer mapped.

            var destinationPtr = DeviceBuffer.Map(0);
            Utilities.Write(destinationPtr + offset, ref data);
            DeviceBuffer.Unmap(0);
        }

        private void PlatformSetData<T>(T[] data, int offset)
            where T : struct
        {
            // TODO: Add API to keep buffer mapped.

            var destinationPtr = DeviceBuffer.Map(0);
            Utilities.Write(destinationPtr + offset, data, 0, data.Length);
            DeviceBuffer.Unmap(0);
        }
    }
}
