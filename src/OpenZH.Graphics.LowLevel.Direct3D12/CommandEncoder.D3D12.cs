using System;
using OpenZH.Graphics.LowLevel.Util;
using SharpDX.Direct3D12;
using SharpDX.Mathematics.Interop;

namespace OpenZH.Graphics.LowLevel
{
    partial class CommandEncoder
    {
        private readonly GraphicsCommandList _commandList;
        private readonly RenderPassDescriptor _renderPassDescriptor;

        internal CommandEncoder(GraphicsDevice graphicsDevice, GraphicsCommandList commandList, RenderPassDescriptor renderPassDescriptor)
            : base(graphicsDevice)
        {
            _commandList = commandList;
            _renderPassDescriptor = renderPassDescriptor;

            _renderPassDescriptor.OnOpenedCommandList(_commandList);
        }

        private void PlatformClose()
        {
            _renderPassDescriptor.OnClosingCommandList(_commandList);

            // Don't close _commandList. We'll close it in D3D12CommandBuffer.Commit.
        }

        private void PlatformDraw(
            PrimitiveType primitiveType,
            uint vertexStart,
            uint vertexCount)
        {
            _commandList.PrimitiveTopology = primitiveType.ToPrimitiveTopology();

            _commandList.DrawInstanced((int) vertexCount, 1, (int) vertexStart, 0);
        }

        private void PlatformDrawIndexed(
            PrimitiveType primitiveType,
            uint indexCount,
            IndexType indexType,
            Buffer indexBuffer,
            uint indexBufferOffset)
        {
            _commandList.PrimitiveTopology = primitiveType.ToPrimitiveTopology();

            _commandList.SetIndexBuffer(new IndexBufferView
            {
                BufferLocation = indexBuffer.DeviceBuffer.GPUVirtualAddress,
                Format = indexType.ToDxgiFormat(),
                SizeInBytes = (int) indexBuffer.DeviceBuffer.Description.Width
            });

            _commandList.DrawIndexedInstanced(
                (int) indexCount,
                1,
                (int) indexBufferOffset,
                0,
                0);
        }

        private void PlatformSetDescriptorSet(int index, DescriptorSet descriptorSet)
        {
            _commandList.SetGraphicsRootDescriptorTable(index, descriptorSet.GPUDescriptorHandleForCbvUavSrvHeapStart);
        }

        private void PlatformSetInlineConstantBuffer(int index, Buffer buffer)
        {
            _commandList.SetGraphicsRootConstantBufferView(index, buffer.DeviceCurrentGPUVirtualAddress);
        }

        private void PlatformSetPipelineState(PipelineState pipelineState)
        {
            _commandList.PipelineState = pipelineState.DevicePipelineState;
            _currentPipelineState = pipelineState;
        }

        private void PlatformSetPipelineLayout(PipelineLayout pipelineLayout)
        {
            _commandList.SetGraphicsRootSignature(pipelineLayout.DeviceRootSignature);
        }

        private void PlatformSetVertexBuffer(int slot, Buffer vertexBuffer)
        {
            if (_currentPipelineState == null)
            {
                throw new InvalidOperationException("Must call SetPipelineState before SetVertexBuffer");
            }

            _commandList.SetVertexBuffer(slot, new VertexBufferView
            {
                BufferLocation = vertexBuffer.DeviceBuffer.GPUVirtualAddress,
                SizeInBytes = (int) vertexBuffer.DeviceBuffer.Description.Width,
                StrideInBytes = _currentPipelineState.Description.VertexDescriptor.GetStride(slot)
            });
        }

        private void PlatformSetViewport(Viewport viewport)
        {
            _commandList.SetViewport(viewport.ToViewportF());
            _commandList.SetScissorRectangles(new RawRectangle(0, 0, viewport.Width, viewport.Height));
        }
    }
}
