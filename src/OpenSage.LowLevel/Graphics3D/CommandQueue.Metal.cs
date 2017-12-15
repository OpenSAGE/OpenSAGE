using Metal;

namespace OpenSage.LowLevel.Graphics3D
{
    partial class CommandQueue
    {
        private IMTLCommandQueue _deviceCommandQueue;

        internal override string PlatformGetDebugName() => _deviceCommandQueue.Label;
        internal override void PlatformSetDebugName(string value) => _deviceCommandQueue.Label = value;

        private void PlatformConstruct(GraphicsDevice graphicsDevice)
        {
            _deviceCommandQueue = AddDisposable(graphicsDevice.Device.CreateCommandQueue());
        }

        private CommandBuffer PlatformGetCommandBuffer()
        {
            return new CommandBuffer(GraphicsDevice, _deviceCommandQueue.CommandBuffer());
        }
    }
}
