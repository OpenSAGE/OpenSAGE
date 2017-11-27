using System;
using LLGfx.Util;
using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;

namespace LLGfx
{
    partial class CommandEncoder
    {
        private readonly DeviceContext _context;
        private readonly RenderPassDescriptor _renderPassDescriptor;

        private PipelineLayout _pipelineLayout;

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
            StaticBuffer<ushort> indexBuffer,
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
            StaticBuffer<ushort> indexBuffer,
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

        private delegate void SetShaderResourcesCallback(CommonShaderStage shaderStage, int slot);

        private void SetShaderResources(int index, SetShaderResourcesCallback callback)
        {
            ref var pipelineLayoutEntry = ref _pipelineLayout.Description.Entries[index];

            CommonShaderStage shaderStage;
            switch (pipelineLayoutEntry.Visibility)
            {
                case ShaderStageVisibility.Vertex:
                    shaderStage = _context.VertexShader;
                    break;

                case ShaderStageVisibility.Pixel:
                    shaderStage = _context.PixelShader;
                    break;

                default:
                    throw new InvalidOperationException();
            }

            switch (pipelineLayoutEntry.EntryType)
            {
                case PipelineLayoutEntryType.Resource:
                    callback(shaderStage, pipelineLayoutEntry.Resource.ShaderRegister);
                    break;

                case PipelineLayoutEntryType.ResourceView:
                    callback(shaderStage, pipelineLayoutEntry.ResourceView.BaseShaderRegister);
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }

        private void PlatformSetTexture(int index, Texture texture)
        {
            SetShaderResources(index, (shaderStage, slot) => shaderStage.SetShaderResource(slot, texture?.DeviceShaderResourceView));
        }

        private void PlatformSetTextures(int index, TextureSet textures)
        {
            SetShaderResources(index, (shaderStage, slot) => shaderStage.SetShaderResources(slot, textures.DeviceShaderResourceViews));
        }

        private void PlatformSetStaticBuffer<T>(int index, StaticBuffer<T> buffer)
            where T : struct
        {
            SetShaderResources(index, (shaderStage, slot) => shaderStage.SetShaderResource(slot, buffer?.DeviceShaderResourceView));
        }

        private void PlatformSetInlineConstantBuffer(int index, Buffer buffer)
        {
            SetShaderResources(index, (shaderStage, slot) => shaderStage.SetConstantBuffer(slot, buffer.DeviceBuffer));
        }

        private void PlatformSetInlineStructuredBuffer(int index, Buffer buffer)
        {
            SetShaderResources(index, (shaderStage, slot) => shaderStage.SetShaderResource(slot, buffer.DeviceShaderResourceView));
        }

        private void PlatformSetPipelineState(PipelineState pipelineState)
        {
            pipelineState.Apply(_context);
        }

        private void PlatformSetPipelineLayout(PipelineLayout pipelineLayout)
        {
            _pipelineLayout = pipelineLayout;
            pipelineLayout.Apply(_context);
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
