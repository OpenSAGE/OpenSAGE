namespace OpenZH.Graphics.LowLevel
{
    public sealed partial class CommandEncoder : GraphicsDeviceChild
    {
        private PipelineState _currentPipelineState;

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
            IndexType indexType,
            Buffer indexBuffer,
            uint indexBufferOffset)
        {
            PlatformDrawIndexed(
                primitiveType,
                indexCount,
                indexType,
                indexBuffer,
                indexBufferOffset);
        }

        public void SetDescriptorSet(int index, DescriptorSet descriptorSet)
        {
            // TODO: Validation.

            PlatformSetDescriptorSet(index, descriptorSet);
        }

        public void SetInlineConstantBuffer(int index, Buffer buffer)
        {
            // TODO: Validation.

            PlatformSetInlineConstantBuffer(index, buffer);
        }

        public void SetPipelineState(PipelineState pipelineState)
        {
            _currentPipelineState = pipelineState;

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
