namespace LLGfx
{
    partial class CommandBuffer
    {
        private readonly CommandQueue _parent;

        internal CommandBuffer(GraphicsDevice graphicsDevice, CommandQueue parent)
            : base(graphicsDevice)
        {
            _parent = parent;
        }

        private CommandEncoder PlatformGetCommandEncoder(RenderPassDescriptor renderPassDescriptor)
        {
            return new CommandEncoder(
                GraphicsDevice,
                _parent.GetOrCreateCommandList(),
                renderPassDescriptor);
        }

        private void PlatformCommit()
        {
            _parent.ExecuteCommandList();
        }

        private void PlatformCommitAndPresent(SwapChain swapChain)
        {
            PlatformCommit();

            swapChain.Present();
        }
    }
}
