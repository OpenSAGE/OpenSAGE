namespace LL.Graphics3D
{
    public sealed partial class Texture : GraphicsDeviceChild
    {
        public int Width { get; }
        public int Height { get; }
        public int MipMapCount { get; }

        public static Texture CreateTexture2D(
            GraphicsDevice graphicsDevice,
            PixelFormat pixelFormat,
            int width,
            int height,
            TextureMipMapData[] mipMapData)
        {
            return new Texture(
                graphicsDevice,
                pixelFormat,
                1,
                width,
                height,
                TextureBindFlags.ShaderResource,
                mipMapData.Length,
                mipMapData);
        }

        public static Texture CreateTexture2DArray(
            GraphicsDevice graphicsDevice,
            PixelFormat pixelFormat,
            int arraySize,
            int mipMapCount,
            int width,
            int height)
        {
            return new Texture(
                graphicsDevice,
                pixelFormat,
                arraySize,
                width,
                height,
                TextureBindFlags.ShaderResource,
                mipMapCount,
                null);
        }

        public static Texture CreatePlaceholderTexture2D(GraphicsDevice graphicsDevice)
        {
            return CreateTexture2D(
                graphicsDevice,
                PixelFormat.Rgba8UNorm,
                1,
                1,
                new[]
                {
                    new TextureMipMapData
                    {
                        BytesPerRow = 4,
                        Data = new byte[] { 255, 105, 180, 255 }
                    }
                });
        }

        public static Texture CreateTexture2D(
            GraphicsDevice graphicsDevice,
            PixelFormat pixelFormat,
            int width,
            int height,
            TextureBindFlags bindFlags)
        {
            var result = new Texture(
                graphicsDevice,
                pixelFormat,
                1,
                width,
                height,
                bindFlags,
                1,
                null);

            return result;
        }

        private Texture(
            GraphicsDevice graphicsDevice,
            PixelFormat pixelFormat,
            int arraySize,
            int width,
            int height,
            TextureBindFlags bindFlags,
            int mipMapCount,
            TextureMipMapData[] mipMapData)
            : base(graphicsDevice)
        {
            Width = width;
            Height = height;
            MipMapCount = mipMapCount;

            PlatformConstruct(
                graphicsDevice,
                pixelFormat,
                arraySize,
                width,
                height,
                bindFlags,
                mipMapCount,
                mipMapData);
        }

        public void CopyFromTexture(
            Texture source,
            int destinationArrayIndex)
        {
            PlatformCopyFromTexture(source, destinationArrayIndex);
        }
    }

    public struct TextureMipMapData
    {
        public byte[] Data;
        public int BytesPerRow;
    }
}
