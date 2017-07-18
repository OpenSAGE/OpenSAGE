namespace OpenZH.Graphics
{
    public abstract class CommandEncoder
    {
        public abstract void Close();

        public abstract void DrawIndexed(
            PrimitiveType primitiveType,
            uint indexCount,
            IndexType indexType,
            Buffer indexBuffer,
            uint indexBufferOffset);

        public abstract void SetViewport(
            Viewport viewport);
    }
}
