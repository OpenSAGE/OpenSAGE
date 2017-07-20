namespace OpenZH.Graphics
{
    public sealed partial class CommandEncoder : GraphicsDeviceChild
    {
        public void Close()
        {
            PlatformClose();
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

        public void SetPipelineState(GraphicsPipelineState pipelineState)
        {
            PlatformSetPipelineState(pipelineState);
        }

        public void SetRootSignature(RootSignature rootSignature)
        {
            PlatformSetRootSignature(rootSignature);
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
