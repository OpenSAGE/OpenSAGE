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
            StaticBuffer<ushort> indexBuffer,
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
            StaticBuffer<ushort> indexBuffer,
            uint indexBufferOffset)
        {
            PlatformDrawIndexedInstanced(
                primitiveType,
                indexCount,
                instanceCount,
                indexBuffer,
                indexBufferOffset);
        }

        public void SetTexture(int index, Texture texture)
        {
            // TODO: Validation.

            PlatformSetTexture(index, texture);
        }

        public void SetStaticBuffer<T>(int index, StaticBuffer<T> buffer)
            where T : struct
        {
            // TODO: Validation.

            PlatformSetStaticBuffer(index, buffer);
        }

        public void SetInlineConstantBuffer(int index, Buffer buffer)
        {
            // TODO: Validation.

            PlatformSetInlineConstantBuffer(index, buffer);
        }

        public void SetInlineStructuredBuffer(int index, Buffer buffer)
        {
            // TODO: Validation.

            PlatformSetInlineStructuredBuffer(index, buffer);
        }

        public void SetPipelineState(PipelineState pipelineState)
        {
            PlatformSetPipelineState(pipelineState);
        }

        public void SetPipelineLayout(PipelineLayout pipelineLayout)
        {
            PlatformSetPipelineLayout(pipelineLayout);
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
