using Metal;

namespace OpenZH.Graphics.Metal
{
    public sealed class MetalCommandQueue : CommandQueue
    {
        private readonly IMTLCommandQueue _commandQueue;

        public MetalCommandQueue(IMTLDevice device)
        {
            _commandQueue = AddDisposable(device.CreateCommandQueue());
        }

        public override CommandBuffer GetCommandBuffer()
        {
            return new MetalCommandBuffer(_commandQueue.CommandBuffer());
        }
    }
}