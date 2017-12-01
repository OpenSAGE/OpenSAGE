using LLGfx.Util;
using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;

namespace LLGfx
{
    partial class CommandEncoder
    {
        private readonly DeviceContext _context;
        private readonly RenderPassDescriptor _renderPassDescriptor;

        internal override string PlatformGetDebugName() => null;
        internal override void PlatformSetDebugName(string value) { }

        internal CommandEncoder(GraphicsDevice graphicsDevice, DeviceContext context, RenderPassDescriptor renderPassDescriptor)
            : base(graphicsDevice)
        {
            _context = context;
            _renderPassDescriptor = renderPassDescriptor;

            _renderPassDescriptor.OnOpenedCommandList(_context);
        }

        private void PlatformClose() { }

        private void PlatformDraw(
            PrimitiveType primitiveType,
            uint vertexStart,
            uint vertexCount)
        {
            _context.InputAssembler.PrimitiveTopology = primitiveType.ToPrimitiveTopology();

            _context.Draw((int) vertexCount, (int) vertexStart);
        }

        private void PlatformDrawIndexed(
            PrimitiveType primitiveType,
            uint indexCount,
            Buffer<ushort> indexBuffer,
            uint indexBufferOffset)
        {
            _context.InputAssembler.PrimitiveTopology = primitiveType.ToPrimitiveTopology();

            _context.InputAssembler.SetIndexBuffer(
                indexBuffer.DeviceBuffer,
                SharpDX.DXGI.Format.R16_UInt,
                0);

            _context.DrawIndexed(
                (int) indexCount,
                (int) indexBufferOffset,
                0);
        }

        private void PlatformDrawIndexedInstanced(
            PrimitiveType primitiveType,
            uint indexCount,
            uint instanceCount,
            Buffer<ushort> indexBuffer,
            uint indexBufferOffset)
        {
            _context.InputAssembler.PrimitiveTopology = primitiveType.ToPrimitiveTopology();

            _context.InputAssembler.SetIndexBuffer(
                indexBuffer.DeviceBuffer,
                SharpDX.DXGI.Format.R16_UInt,
                0);

            _context.DrawIndexedInstanced(
                (int) indexCount,
                (int) instanceCount,
                (int) indexBufferOffset,
                0,
                0);
        }

        private void PlatformSetVertexShaderTexture(int slot, Texture texture)
        {
            _context.VertexShader.SetShaderResource(slot, texture?.DeviceShaderResourceView);
        }

        private void PlatformSetPixelShaderTexture(int slot, Texture texture)
        {
            _context.PixelShader.SetShaderResource(slot, texture?.DeviceShaderResourceView);
        }

        private void PlatformSetVertexShaderSampler(int slot, SamplerState sampler)
        {
            _context.VertexShader.SetSampler(slot, sampler.DeviceSamplerState);
        }

        private void PlatformSetPixelShaderSampler(int slot, SamplerState sampler)
        {
            _context.PixelShader.SetSampler(slot, sampler.DeviceSamplerState);
        }

        private void PlatformSetVertexShaderStructuredBuffer(int slot, Buffer buffer)
        {
            _context.VertexShader.SetShaderResource(slot, buffer?.DeviceShaderResourceView);
        }

        private void PlatformSetPixelShaderStructuredBuffer(int slot, Buffer buffer)
        {
            _context.PixelShader.SetShaderResource(slot, buffer?.DeviceShaderResourceView);
        }

        private void PlatformSetVertexShaderConstantBuffer(int slot, Buffer buffer)
        {
            _context.VertexShader.SetConstantBuffer(slot, buffer.DeviceBuffer);
        }

        private void PlatformSetPixelShaderConstantBuffer(int slot, Buffer buffer)
        {
            _context.PixelShader.SetConstantBuffer(slot, buffer.DeviceBuffer);
        }

        private void PlatformSetPipelineState(PipelineState pipelineState)
        {
            pipelineState.Apply(_context);
        }

        private void PlatformSetVertexBuffer(int slot, Buffer vertexBuffer)
        {
            _context.InputAssembler.SetVertexBuffers(
                slot,
                new VertexBufferBinding(
                    vertexBuffer.DeviceBuffer,
                    (int) vertexBuffer.ElementSizeInBytes,
                    0));
        }

        private void PlatformSetViewport(Viewport viewport)
        {
            _context.Rasterizer.SetViewport(viewport.ToViewportF());
            _context.Rasterizer.SetScissorRectangles(new RawRectangle(0, 0, viewport.Width, viewport.Height));
        }
    }
}
