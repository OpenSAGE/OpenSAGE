namespace LL.Graphics3D
{
    public sealed partial class CommandBuffer : GraphicsDeviceChild
    {
        public CommandEncoder GetCommandEncoder(RenderPassDescriptor renderPassDescriptor)
        {
            return PlatformGetCommandEncoder(renderPassDescriptor);
        }

        public void Commit()
        {
            PlatformCommit();
        }

        public void CommitAndPresent(SwapChain swapChain)
        {
            PlatformCommitAndPresent(swapChain);
        }
    }
}
