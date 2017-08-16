using Metal;

namespace OpenZH.Graphics.LowLevel
{
    partial class CommandQueue
    {
        private IMTLCommandQueue _commandQueue;

        private void PlatformConstruct(GraphicsDevice graphicsDevice)
        {
            _commandQueue = AddDisposable(graphicsDevice.Device.CreateCommandQueue());
        }

        private CommandBuffer PlatformGetCommandBuffer()
        {
            return new CommandBuffer(GraphicsDevice, _commandQueue.CommandBuffer());
        }
    }
}