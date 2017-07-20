using Metal;

namespace OpenZH.Graphics
{
    partial class Buffer
    {
        internal IMTLBuffer DeviceBuffer { get; private set; }

        private void PlatformConstruct(GraphicsDevice graphicsDevice, uint sizeInBytes)
        {
            DeviceBuffer = AddDisposable(graphicsDevice.Device.CreateBuffer(
                sizeInBytes,
                MTLResourceOptions.CpuCacheModeDefault));
        }
    }
}