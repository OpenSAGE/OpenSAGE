namespace LLGfx
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

        public void SetVertexTexture(int slot, Texture texture)
        {
            PlatformSetVertexTexture(slot, texture);
        }

        public void SetFragmentTexture(int slot, Texture texture)
        {
            PlatformSetFragmentTexture(slot, texture);
        }

        public void SetVertexSampler(int slot, SamplerState sampler)
        {
            PlatformSetVertexSampler(slot, sampler);
        }

        public void SetFragmentSampler(int slot, SamplerState sampler)
        {
            PlatformSetFragmentSampler(slot, sampler);
        }

        public void SetVertexStructuredBuffer(int slot, Buffer buffer)
        {
            PlatformSetVertexStructuredBuffer(slot, buffer);
        }

        public void SetFragmentStructuredBuffer(int slot, Buffer buffer)
        {
            PlatformSetFragmentStructuredBuffer(slot, buffer);
        }

        public void SetVertexConstantBuffer(int slot, Buffer buffer)
        {
            PlatformSetVertexConstantBuffer(slot, buffer);
        }

        public void SetFragmentConstantBuffer(int slot, Buffer buffer)
        {
            PlatformSetFragmentConstantBuffer(slot, buffer);
        }

        public void SetPipelineState(PipelineState pipelineState)
        {
            PlatformSetPipelineState(pipelineState);
        }

        public void SetVertexBuffer(int bufferIndex, Buffer vertexBuffer)
        {
            PlatformSetVertexBuffer(bufferIndex, vertexBuffer);
        }

        public void SetViewport(Viewport viewport)
        {
            PlatformSetViewport(viewport);
        }
    }
}
