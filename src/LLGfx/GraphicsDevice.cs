namespace LLGfx
{
    public sealed partial class GraphicsDevice : GraphicsObject
    {
        public CommandQueue CommandQueue { get; }

        public PixelFormat BackBufferFormat => PlatformBackBufferFormat;

        public GraphicsDevice()
        {
            PlatformConstruct();

            CommandQueue = AddDisposable(new CommandQueue(this));
        }
    }
}
