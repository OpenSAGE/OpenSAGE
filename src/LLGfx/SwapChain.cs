namespace LLGfx
{
    public sealed partial class SwapChain : GraphicsObject
    {
        public PixelFormat BackBufferFormat => PlatformBackBufferFormat;

        public double BackBufferWidth => PlatformBackBufferWidth;
        public double BackBufferHeight => PlatformBackBufferHeight;

        public RenderTarget GetNextRenderTarget()
        {
            return PlatformGetNextRenderTarget();
        }
    }
}
