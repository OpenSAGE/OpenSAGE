namespace LLGfx
{
    public sealed partial class GraphicsDevice : GraphicsObject
    {
        public CommandQueue CommandQueue { get; }

        public ShaderLibrary ShaderLibrary { get; }

        public PixelFormat BackBufferFormat => PlatformBackBufferFormat;

        public GraphicsDevice()
        {
            PlatformConstruct();

            CommandQueue = AddDisposable(new CommandQueue(this));

            ShaderLibrary = AddDisposable(new ShaderLibrary(this));
        }
    }
}
