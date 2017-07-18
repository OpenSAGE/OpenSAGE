namespace OpenZH.Graphics.Direct3D12
{
    public sealed class D3D12CommandBuffer : CommandBuffer
    {
        private readonly D3D12CommandQueue _parent;

        public D3D12CommandBuffer(D3D12CommandQueue parent)
        {
            _parent = parent;
        }

        public override CommandEncoder GetCommandEncoder(RenderPassDescriptor renderPassDescriptor)
        {
            return new D3D12CommandEncoder(
                _parent.GetOrCreateCommandList(),
                (D3D12RenderPassDescriptor) renderPassDescriptor);
        }

        public override void Commit()
        {
            _parent.ExecuteCommandList();
        }

        public override void CommitAndPresent(SwapChain swapChain)
        {
            Commit();

            ((D3D12SwapChain) swapChain).Present();
        }
    }
}
