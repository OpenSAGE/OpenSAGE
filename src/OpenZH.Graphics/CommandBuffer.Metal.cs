using Metal;

namespace OpenZH.Graphics
{
    partial class CommandBuffer
    {
        private readonly IMTLCommandBuffer _commandBuffer;

        internal CommandBuffer(GraphicsDevice graphicsDevice, IMTLCommandBuffer commandBuffer)
            : base(graphicsDevice)
        {
            _commandBuffer = commandBuffer;
        }

        private CommandEncoder PlatformGetCommandEncoder(RenderPassDescriptor renderPassDescriptor)
        {
            var deviceRenderPassDescriptor = renderPassDescriptor.DeviceDescriptor;

            var deviceRenderCommandEncoder = _commandBuffer.CreateRenderCommandEncoder(deviceRenderPassDescriptor);

            return new CommandEncoder(GraphicsDevice, deviceRenderCommandEncoder);
        }

        private void PlatformCommit()
        {
            _commandBuffer.Commit();
        }

        private void PlatformCommitAndPresent(SwapChain swapChain)
        {
            _commandBuffer.PresentDrawable(swapChain.CurrentDrawable);

            Commit();
        }
    }
}