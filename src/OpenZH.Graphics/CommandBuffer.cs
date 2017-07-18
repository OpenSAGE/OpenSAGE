namespace OpenZH.Graphics
{
    public abstract class CommandBuffer
    {
        public abstract CommandEncoder GetCommandEncoder(RenderPassDescriptor renderPassDescriptor);

        public abstract void Commit();

        public abstract void CommitAndPresent(SwapChain swapChain);
    }
}
