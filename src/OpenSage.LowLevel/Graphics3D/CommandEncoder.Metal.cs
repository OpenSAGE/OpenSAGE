using System;
using Metal;
using OpenSage.LowLevel.Graphics3D.Util;

namespace OpenSage.LowLevel.Graphics3D
{
    partial class CommandEncoder
    {
        private readonly IMTLRenderCommandEncoder _deviceCommandEncoder;
        private readonly RenderPassDescriptor _renderPassDescriptor;

        internal override string PlatformGetDebugName() => null;
        internal override void PlatformSetDebugName(string value) { }

        internal CommandEncoder(GraphicsDevice graphicsDevice, IMTLRenderCommandEncoder deviceCommandEncoder)
            : base(graphicsDevice)
        {
            _deviceCommandEncoder = deviceCommandEncoder;
        }

        private void PlatformClose()
        {
            _deviceCommandEncoder.EndEncoding();
        }

        private void PlatformDraw(
            PrimitiveType primitiveType,
            uint vertexStart,
            uint vertexCount)
        {
            _deviceCommandEncoder.DrawPrimitives(
                primitiveType.ToMTLPrimitiveType(),
                vertexStart,
                vertexCount);
        }

        private void PlatformDrawIndexed(
            PrimitiveType primitiveType,
            uint indexCount,
            Buffer<ushort> indexBuffer,
            uint indexBufferOffset)
        {
            _deviceCommandEncoder.DrawIndexedPrimitives(
                primitiveType.ToMTLPrimitiveType(),
                indexCount,
                MTLIndexType.UInt16,
                indexBuffer.DeviceBuffer,
                indexBufferOffset);
        }

        private void PlatformDrawIndexedInstanced(
            PrimitiveType primitiveType,
            uint indexCount,
            uint instanceCount,
            Buffer<ushort> indexBuffer,
            uint indexBufferOffset)
        {
            _deviceCommandEncoder.DrawIndexedPrimitives(
                primitiveType.ToMTLPrimitiveType(),
                indexCount,
                MTLIndexType.UInt16,
                indexBuffer.DeviceBuffer,
                indexBufferOffset,
                instanceCount);
        }

        private void PlatformSetVertexShaderTexture(int slot, Texture texture)
        {
            _deviceCommandEncoder.SetVertexTexture(texture?.DeviceTexture, (nuint) slot);
        }

        private void PlatformSetPixelShaderTexture(int slot, Texture texture)
        {
            _deviceCommandEncoder.SetFragmentTexture(texture?.DeviceTexture, (nuint) slot);
        }

        private void PlatformSetVertexShaderSampler(int slot, SamplerState sampler)
        {
            _deviceCommandEncoder.SetVertexSamplerState(sampler.DeviceSamplerState, (nuint) slot);
        }

        private void PlatformSetPixelShaderSampler(int slot, SamplerState sampler)
        {
            _deviceCommandEncoder.SetFragmentSamplerState(sampler.DeviceSamplerState, (nuint) slot);
        }

        private void PlatformSetVertexShaderStructuredBuffer(int slot, Buffer buffer)
        {
            _deviceCommandEncoder.SetVertexBuffer(buffer?.DeviceBuffer, 0, (nuint) slot);
        }

        private void PlatformSetPixelShaderStructuredBuffer(int slot, Buffer buffer)
        {
            _deviceCommandEncoder.SetFragmentBuffer(buffer?.DeviceBuffer, 0, (nuint) slot);
        }

        private void PlatformSetVertexShaderConstantBuffer(int slot, Buffer buffer)
        {
            _deviceCommandEncoder.SetVertexBuffer(buffer?.DeviceBuffer, 0, (nuint) slot);
        }

        private void PlatformSetPixelShaderConstantBuffer(int slot, Buffer buffer)
        {
            _deviceCommandEncoder.SetFragmentBuffer(buffer?.DeviceBuffer, 0, (nuint) slot);
        }

        private void PlatformSetPipelineState(PipelineState pipelineState)
        {
            _deviceCommandEncoder.SetRenderPipelineState(pipelineState.DeviceRenderPipelineState);

            pipelineState.Apply(_deviceCommandEncoder);
        }

        private void PlatformSetVertexBuffer(int slot, Buffer vertexBuffer)
        {
            _deviceCommandEncoder.SetVertexBuffer(vertexBuffer?.DeviceBuffer, 0, (nuint) slot);
        }

        private void PlatformSetViewport(Viewport viewport)
        {
            _deviceCommandEncoder.SetViewport(viewport.ToMTLViewport());
            _deviceCommandEncoder.SetScissorRect(new MTLScissorRect(0, 0, (nuint) viewport.Width, (nuint) viewport.Height));
        }
    }
}
