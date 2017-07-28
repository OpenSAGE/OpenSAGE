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

        private void PlatformSetConstantBuffer(int index, Buffer buffer)
        {
            // TODO: Lookup in current root signature whether this buffer
            // should be bound to vertex shader, fragment shader, or both.

            //_commandList.SetGraphicsRootConstantBufferView(index, buffer.DeviceBuffer.GPUVirtualAddress);
        }

        private void PlatformSetDescriptorSet(int index, DescriptorSet descriptorSet)
        {
            // TODO
        }

        private void PlatformSetPipelineState(PipelineState pipelineState)
        {
            _commandEncoder.SetRenderPipelineState(pipelineState.DeviceRenderPipelineState);
        }

        private void PlatformSetPipelineLayout(PipelineLayout pipelineLayout)
        {
            for (var i = 0; i < pipelineLayout.Description.StaticSamplerStates.Length; i++)
            {
                var staticSamplerState = pipelineLayout.Description.StaticSamplerStates[i];

                switch (staticSamplerState.Visibility)
                {
                    case ShaderStageVisibility.Vertex:
                        _commandEncoder.SetVertexSamplerState(pipelineLayout.DeviceSamplerStates[i], (nuint) staticSamplerState.ShaderRegister);
                        break;

                    case ShaderStageVisibility.Pixel:
                        _commandEncoder.SetFragmentSamplerState(pipelineLayout.DeviceSamplerStates[i], (nuint) staticSamplerState.ShaderRegister);
                        break;

                    case ShaderStageVisibility.All:
                        _commandEncoder.SetVertexSamplerState(pipelineLayout.DeviceSamplerStates[i], (nuint) staticSamplerState.ShaderRegister);
                        _commandEncoder.SetFragmentSamplerState(pipelineLayout.DeviceSamplerStates[i], (nuint) staticSamplerState.ShaderRegister);
                        break;
                }
            }
        }

        private void PlatformSetVertexBuffer(int bufferIndex, Buffer vertexBuffer)
        {
            _commandEncoder.SetVertexBuffer(vertexBuffer.DeviceBuffer, 0, (nuint) bufferIndex);
        }

        private void PlatformDraw(
            PrimitiveType primitiveType,
            uint vertexStart,
            uint vertexCount)
        {
            _commandEncoder.DrawPrimitives(
                primitiveType.ToMTLPrimitiveType(),
                vertexStart,
                vertexCount);
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