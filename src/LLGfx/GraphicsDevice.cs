namespace LLGfx
{
    public sealed partial class GraphicsDevice : GraphicsObject
    {
        public CommandQueue CommandQueue { get; }

        public ShaderLibrary ShaderLibrary { get; }

        public PixelFormat BackBufferFormat => PlatformBackBufferFormat;

        internal ShaderResourceView NullBufferShaderResourceView { get; }
        internal ShaderResourceView NullTextureShaderResourceView { get; }

        public GraphicsDevice()
        {
            PlatformConstruct();

            CommandQueue = AddDisposable(new CommandQueue(this));

            ShaderLibrary = AddDisposable(new ShaderLibrary(this));

            NullBufferShaderResourceView = AddDisposable(ShaderResourceView.Create(this, (StaticBuffer<uint>) null));
            NullTextureShaderResourceView = AddDisposable(ShaderResourceView.Create(this, (Texture) null));
        }
    }
}
