namespace LLGfx
{
    public sealed partial class Texture : GraphicsDeviceChild
    {
        public int Width { get; }
        public int Height { get; }
        public int MipMapCount { get; }

        private ShaderResourceView _shaderResourceView;

        internal ShaderResourceView ShaderResourceView
        {
            get
            {
                if (_shaderResourceView == null)
                {
                    _shaderResourceView = AddDisposable(ShaderResourceView.Create(GraphicsDevice, this));
                }
                return _shaderResourceView;
            }
        }

        public static Texture CreateTexture2D(
            GraphicsDevice graphicsDevice,
            ResourceUploadBatch uploadBatch,
            PixelFormat pixelFormat,
            int width,
            int height,
            TextureMipMapData[] mipMapData)
        {
            var result = new Texture(
                graphicsDevice,
                uploadBatch,
                pixelFormat,
                1,
                width,
                height,
                mipMapData.Length);

            result.SetData(
                uploadBatch,
                0,
                mipMapData);

            return result;
        }

        public static Texture CreateTexture2DArray(
            GraphicsDevice graphicsDevice,
            ResourceUploadBatch uploadBatch,
            PixelFormat pixelFormat,
            int arraySize,
            int mipMapCount,
            int width,
            int height)
        {
            return new Texture(
                graphicsDevice,
                uploadBatch,
                pixelFormat,
                arraySize,
                width,
                height,
                mipMapCount);
        }

        public static Texture CreatePlaceholderTexture2D(
            GraphicsDevice graphicsDevice,
            ResourceUploadBatch uploadBatch)
        {
            return CreateTexture2D(
                graphicsDevice,
                uploadBatch,
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

        private Texture(
            GraphicsDevice graphicsDevice,
            ResourceUploadBatch uploadBatch,
            PixelFormat pixelFormat,
            int arraySize,
            int width,
            int height,
            int mipMapCount)
            : base(graphicsDevice)
        {
            Width = width;
            Height = height;
            MipMapCount = mipMapCount;

            PlatformConstruct(
                graphicsDevice,
                uploadBatch,
                pixelFormat,
                arraySize,
                width,
                height,
                mipMapCount);
        }

        public void SetData(
            ResourceUploadBatch uploadBatch, 
            int arrayIndex,
            TextureMipMapData[] mipMapData)
        {
            PlatformSetData(uploadBatch, arrayIndex, mipMapData);
        }

        public void Freeze(ResourceUploadBatch uploadBatch)
        {
            PlatformFreeze(uploadBatch);
        }
    }

    public struct TextureMipMapData
    {
        public byte[] Data;
        public int BytesPerRow;
    }
}
