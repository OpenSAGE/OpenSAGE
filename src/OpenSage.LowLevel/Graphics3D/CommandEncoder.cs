namespace OpenSage.LowLevel.Graphics3D
{
    public sealed partial class CommandEncoder : GraphicsDeviceChild
    {
        public void Close()
        {
            PlatformClose();
        }

        public void Draw(
            PrimitiveType primitiveType,
            uint vertexStart,
            uint vertexCount)
        {
            PlatformDraw(
                primitiveType,
                vertexStart,
                vertexCount);
        }

        public void DrawIndexed(
            PrimitiveType primitiveType,
            uint indexCount,
            Buffer<ushort> indexBuffer,
            uint indexBufferOffset)
        {
            PlatformDrawIndexed(
                primitiveType,
                indexCount,
                indexBuffer,
                indexBufferOffset);
        }

        public void DrawIndexedInstanced(
            PrimitiveType primitiveType,
            uint indexCount,
            uint instanceCount,
            Buffer<ushort> indexBuffer,
            uint indexBufferOffset)
        {
            PlatformDrawIndexedInstanced(
                primitiveType,
                indexCount,
                instanceCount,
                indexBuffer,
                indexBufferOffset);
        }

        public void SetVertexShaderTexture(int slot, Texture texture)
        {
            PlatformSetVertexShaderTexture(slot, texture);
        }

        public void SetPixelShaderTexture(int slot, Texture texture)
        {
            PlatformSetPixelShaderTexture(slot, texture);
        }

        public void SetVertexShaderSampler(int slot, SamplerState sampler)
        {
            PlatformSetVertexShaderSampler(slot, sampler);
        }

        public void SetPixelShaderSampler(int slot, SamplerState sampler)
        {
            PlatformSetPixelShaderSampler(slot, sampler);
        }

        public void SetVertexShaderStructuredBuffer(int slot, Buffer buffer)
        {
            PlatformSetVertexShaderStructuredBuffer(slot, buffer);
        }

        public void SetPixelShaderStructuredBuffer(int slot, Buffer buffer)
        {
            PlatformSetPixelShaderStructuredBuffer(slot, buffer);
        }

        public void SetVertexShaderConstantBuffer(int slot, Buffer buffer)
        {
            PlatformSetVertexShaderConstantBuffer(slot, buffer);
        }

        public void SetPixelShaderConstantBuffer(int slot, Buffer buffer)
        {
            PlatformSetPixelShaderConstantBuffer(slot, buffer);
        }

        public void SetPipelineState(PipelineState pipelineState)
        {
            PlatformSetPipelineState(pipelineState);
        }

        public void SetVertexBuffer(int bufferIndex, Buffer vertexBuffer)
        {
            PlatformSetVertexBuffer(bufferIndex, vertexBuffer);
        }

        public void SetViewport(in Viewport viewport)
        {
            PlatformSetViewport(viewport);
        }
    }
}
