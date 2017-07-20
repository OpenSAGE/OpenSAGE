namespace OpenZH.Graphics
{
    public sealed partial class Texture : GraphicsDeviceChild
    {
        public static Texture CreateTexture2D(
            GraphicsDevice graphicsDevice,
            PixelFormat pixelFormat,
            int width,
            int height,
            int numMipmapLevels)
        {
            return new Texture(
                graphicsDevice,
                pixelFormat,
                width,
                height,
                numMipmapLevels);
        }

        private Texture(
            GraphicsDevice graphicsDevice,
            PixelFormat pixelFormat,
            int width,
            int height,
            int numMipmapLevels)
            : base(graphicsDevice)
        {
            PlatformConstruct(
                graphicsDevice,
                pixelFormat,
                width,
                height,
                numMipmapLevels);
        }

        public void SetData(
            ResourceUploadBatch uploadBatch,
            int level,
            byte[] data,
            int bytesPerRow)
        {
            PlatformSetData(
                uploadBatch,
                level,
                data,
                bytesPerRow);
        }
    }
}
