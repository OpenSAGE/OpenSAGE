using Metal;

namespace OpenZH.Graphics.Metal
{
    public sealed class MetalCommandBuffer : CommandBuffer
    {
        private readonly IMTLCommandBuffer _commandBuffer;

        public MetalCommandBuffer(IMTLCommandBuffer commandBuffer)
        {
            _commandBuffer = commandBuffer;
        }

        public override CommandEncoder GetCommandEncoder(RenderPassDescriptor renderPassDescriptor)
        {
            var deviceRenderPassDescriptor = ((MetalRenderPassDescriptor) renderPassDescriptor).Descriptor;

            var deviceRenderCommandEncoder = _commandBuffer.CreateRenderCommandEncoder(deviceRenderPassDescriptor);

            return new MetalCommandEncoder(deviceRenderCommandEncoder);
        }

        public override void Commit()
        {
            _commandBuffer.Commit();
        }

        public override void CommitAndPresent(SwapChain swapChain)
        {
            _commandBuffer.PresentDrawable(((MetalSwapChain) swapChain).CurrentDrawable);

            Commit();
        }
    }
}