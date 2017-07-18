using SharpDX.Direct3D12;
using SharpDX.Mathematics.Interop;

namespace OpenZH.Graphics.Direct3D12
{
    public class D3D12CommandEncoder : CommandEncoder
    {
        private readonly GraphicsCommandList _commandList;
        private readonly D3D12RenderPassDescriptor _renderPassDescriptor;

        internal D3D12CommandEncoder(GraphicsCommandList commandList, D3D12RenderPassDescriptor renderPassDescriptor)
        {
            _commandList = commandList;
            _renderPassDescriptor = renderPassDescriptor;

            _renderPassDescriptor.OnOpenedCommandList(_commandList);
        }

        public override void Close()
        {
            _renderPassDescriptor.OnClosingCommandList(_commandList);

            // Don't close _commandList. We'll close it in D3D12CommandBuffer.Commit.
        }

        public override void DrawIndexed(
            PrimitiveType primitiveType,
            uint indexCount,
            IndexType indexType,
            Buffer indexBuffer,
            uint indexBufferOffset)
        {
            _commandList.PrimitiveTopology = primitiveType.ToPrimitiveTopology();

            var d3d12Buffer = (D3D12Buffer) indexBuffer;
            _commandList.SetIndexBuffer(new IndexBufferView
            {
                BufferLocation = d3d12Buffer.Buffer.GPUVirtualAddress,
                Format = indexType.ToDxgiFormat(),
                SizeInBytes = (int) d3d12Buffer.Buffer.Description.Width
            });

            _commandList.DrawIndexedInstanced(
                (int) indexCount,
                1,
                (int) indexBufferOffset,
                0,
                0);
        }

        public override void SetViewport(Viewport viewport)
        {
            _commandList.SetViewport(viewport.ToViewportF());
            _commandList.SetScissorRectangles(new RawRectangle(0, 0, viewport.Width, viewport.Height));
        }
    }
}
