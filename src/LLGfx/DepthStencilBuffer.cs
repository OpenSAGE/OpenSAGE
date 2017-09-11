namespace LLGfx
{
    public sealed partial class DepthStencilBuffer : GraphicsDeviceChild
    {
        public int Width { get; }
        public int Height { get; }

        public DepthStencilBuffer(GraphicsDevice graphicsDevice, int width, int height)
            : base(graphicsDevice)
        {
            Width = width;
            Height = height;

            PlatformConstruct(graphicsDevice, width, height);
        }
    }
}
