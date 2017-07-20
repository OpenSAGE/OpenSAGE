using System;
using Metal;
using OpenZH.Graphics.Platforms.Metal;

namespace OpenZH.Graphics
{
    partial class CommandEncoder
    {
        private readonly IMTLRenderCommandEncoder _commandEncoder;

        internal CommandEncoder(GraphicsDevice graphicsDevice, IMTLRenderCommandEncoder commandEncoder)
            : base(graphicsDevice)
        {
            _commandEncoder = commandEncoder;
        }

        private void PlatformClose()
        {
            _commandEncoder.EndEncoding();
        }

        private void PlatformSetPipelineState(GraphicsPipelineState pipelineState)
        {
            _commandEncoder.SetRenderPipelineState(pipelineState.DeviceRenderPipelineState);
        }

        private void PlatformSetRootSignature(RootSignature rootSignature) { }

        private void PlatformSetVertexBuffer(int bufferIndex, Buffer vertexBuffer)
        {
            _commandEncoder.SetVertexBuffer(vertexBuffer.DeviceBuffer, 0, (nuint) bufferIndex);
        }

        private void PlatformDrawIndexed(
            PrimitiveType primitiveType,
            uint indexCount,
            IndexType indexType,
            Buffer indexBuffer,
            uint indexBufferOffset)
        {
            _commandEncoder.DrawIndexedPrimitives(
                primitiveType.ToMTLPrimitiveType(),
                indexCount,
                indexType.ToMTLIndexType(),
                indexBuffer.DeviceBuffer,
                indexBufferOffset);
        }

        private void PlatformSetViewport(Viewport viewport)
        {
            _commandEncoder.SetViewport(viewport.ToMTLViewport());
            _commandEncoder.SetScissorRect(new MTLScissorRect(0, 0, (nuint) viewport.Width, (nuint) viewport.Height));
        }
    }
}