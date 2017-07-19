namespace OpenZH.Graphics
{
    public abstract class GraphicsDevice : GraphicsObject
    {
        public abstract CommandQueue CommandQueue { get; }

        public abstract RenderPassDescriptor CreateRenderPassDescriptor();

        public abstract ResourceUploadBatch CreateResourceUploadBatch();

        public abstract Texture CreateTexture2D(
            PixelFormat pixelFormat,
            int width,
            int height,
            int numMipmapLevels);
    }
}
