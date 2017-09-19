using System;
using LLGfx.Util;
using SharpDX.Direct3D12;
using SharpDX.Mathematics.Interop;

namespace LLGfx
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
            StaticBuffer<ushort> indexBuffer,
            uint indexBufferOffset)
        {
            _commandList.PrimitiveTopology = primitiveType.ToPrimitiveTopology();

            _commandList.SetIndexBuffer(new IndexBufferView
            {
                BufferLocation = indexBuffer.DeviceBuffer.GPUVirtualAddress,
                Format = SharpDX.DXGI.Format.R16_UInt,
                SizeInBytes = (int) indexBuffer.DeviceBuffer.Description.Width
            });

            _commandList.DrawIndexedInstanced(
                (int) indexCount,
                1,
                (int) indexBufferOffset,
                0,
                0);
        }

        private void PlatformSetShaderResourceView(int index, ShaderResourceView shaderResourceView)
        {
            _commandList.SetGraphicsRootDescriptorTable(index, shaderResourceView.GPUDescriptorHandleForCbvUavSrvHeapStart);
        }

        private void PlatformSetInlineConstantBuffer(int index, Buffer buffer)
        {
            _commandList.SetGraphicsRootConstantBufferView(index, buffer.DeviceCurrentGPUVirtualAddress);
        }

        private void PlatformSetPipelineState(PipelineState pipelineState)
        {
            _commandList.PipelineState = pipelineState.DevicePipelineState;
        }

        private void PlatformSetPipelineLayout(PipelineLayout pipelineLayout)
        {
            _commandList.SetGraphicsRootSignature(pipelineLayout.DeviceRootSignature);
        }

        private void PlatformSetVertexBuffer<T>(int slot, StaticBuffer<T> vertexBuffer)
            where T : struct
        {
            _commandList.SetVertexBuffer(slot, new VertexBufferView
            {
                BufferLocation = vertexBuffer.DeviceBuffer.GPUVirtualAddress,
                SizeInBytes = (int) vertexBuffer.DeviceBuffer.Description.Width,
                StrideInBytes = (int) vertexBuffer.ElementSizeInBytes
            });
        }

        private void PlatformSetViewport(Viewport viewport)
        {
            _commandList.SetViewport(viewport.ToViewportF());
            _commandList.SetScissorRectangles(new RawRectangle(0, 0, viewport.Width, viewport.Height));
        }
    }
}
